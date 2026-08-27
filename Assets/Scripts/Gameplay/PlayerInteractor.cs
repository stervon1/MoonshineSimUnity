using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// Camera-forward raycast interaction. Lives on (or under) the player's
    /// camera. Highlights the focused <see cref="IInteractable"/>, shows its
    /// prompt on <see cref="promptLabel"/>, brightens <see cref="reticle"/>, and
    /// activates the target on E / left-click. Also carries one
    /// <see cref="Carryable"/> at a time (E on empty space drops it).
    ///
    /// This is the game's only interaction verb — there is no action UI.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private float range = 3.5f;
        [SerializeField] private LayerMask mask = ~0;
        [SerializeField] private Text promptLabel;
        [SerializeField] private Graphic reticle;
        [SerializeField] private Transform holdPoint;

        public Carryable Carried { get; private set; }

        private IInteractable _focused;
        private Camera _cam;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            if (_cam == null) _cam = GetComponentInParent<Camera>();
            if (_cam == null) _cam = Camera.main;
            SetPrompt(null);
        }

        private Component _focusedComponent;
        private StationInfoCard _openCard;

        private void Update()
        {
            IInteractable hit = Raycast();
            if (!ReferenceEquals(hit, _focused))
            {
                _focused?.SetFocused(false);
                _focused = hit;
                _focused?.SetFocused(true);
                _focusedComponent = hit as Component;
            }
            SetPrompt(_focused);

            var kb = Keyboard.current;
            var mouse = Mouse.current;

            // Right-click: toggle the "what does this step do" card on the focused station.
            if (mouse != null && mouse.rightButton.wasPressedThisFrame)
            {
                var card = _focusedComponent != null
                    ? _focusedComponent.GetComponentInParent<StationInfoCard>()
                    : null;
                if (_openCard != null && _openCard != card) _openCard.Hide();
                if (card != null)
                {
                    card.Toggle();
                    _openCard = card.IsOpen ? card : null;
                }
                else
                {
                    _openCard = null;
                }
            }

            bool activate = (kb != null && kb.eKey.wasPressedThisFrame) ||
                            (mouse != null && mouse.leftButton.wasPressedThisFrame);
            if (!activate) return;

            if (_focused != null)
            {
                _focused.Interact(this);
            }
            else if (Carried != null)
            {
                Drop();
            }
        }

        // --- carry -----------------------------------------------------------

        public void PickUp(Carryable c)
        {
            if (c == null || c == Carried) return;
            if (Carried != null) Drop();
            Carried = c;
            c.OnPickedUp(holdPoint != null ? holdPoint : transform);
        }

        public void Drop()
        {
            if (Carried == null) return;
            Vector3 p = _cam != null
                ? _cam.transform.position + _cam.transform.forward * 0.9f + Vector3.down * 0.3f
                : transform.position;
            Carried.OnDropped(p);
            Carried = null;
        }

        /// <summary>Remove the carried item from the player without dropping it in the world.</summary>
        public void ClearCarried(bool returnHome)
        {
            if (Carried == null) return;
            if (returnHome) Carried.ReturnHome();
            Carried = null;
        }

        // --- internals -----------------------------------------------------

        private IInteractable Raycast()
        {
            if (_cam == null) return null;
            int layers = mask.value == 0 ? Physics.DefaultRaycastLayers : mask.value;
            var ray = new Ray(_cam.transform.position, _cam.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit info, range, layers, QueryTriggerInteraction.Ignore))
            {
                return null;
            }
            return info.collider.GetComponentInParent<IInteractable>();
        }

        private void SetPrompt(IInteractable target)
        {
            string text = target?.GetPrompt();
            if (promptLabel != null) promptLabel.text = text ?? string.Empty;
            if (reticle != null)
            {
                Color c = reticle.color;
                c.a = string.IsNullOrEmpty(text) ? 0.35f : 1f;
                reticle.color = c;
            }
        }
    }
}
