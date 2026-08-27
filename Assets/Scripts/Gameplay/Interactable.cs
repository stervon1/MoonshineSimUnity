using UnityEngine;
using UnityEngine.Events;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// A look-at-and-activate world object. <see cref="PlayerInteractor"/>
    /// raycasts to find the focused Interactable, shows its <see cref="Verb"/>
    /// as a prompt, and calls <see cref="Activate"/> on input.
    ///
    /// No hard-fail: activating when the underlying action isn't valid is a
    /// downstream no-op (e.g. StillRunController gates its own cut phase).
    /// During the prototype these are plain white blocks near the still.
    /// </summary>
    public class Interactable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string verb = "Use";

        [Tooltip("Invoked on activate. Wire to StillRunController.StartRun / CallCutToHearts / CallCutToTails.")]
        public UnityEvent onActivate = new UnityEvent();

        [Header("Highlight")]
        [SerializeField] private Color focusColor = new Color(1f, 0.85f, 0.4f);
        [SerializeField] private float activateFlashSeconds = 0.15f;

        public string Verb => verb;

        private Renderer _renderer;
        private MaterialPropertyBlock _mpb;
        private Color _baseColor = Color.white;
        private float _flashUntil;
        private bool _focused;

        private void Awake()
        {
            _renderer = GetComponentInChildren<Renderer>();
            _mpb = new MaterialPropertyBlock();

            var mat = _renderer != null ? _renderer.sharedMaterial : null;
            if (mat != null)
            {
                if (mat.HasProperty("_BaseColor")) _baseColor = mat.GetColor("_BaseColor");
                else if (mat.HasProperty("_Color")) _baseColor = mat.GetColor("_Color");
            }
        }

        public string GetPrompt() => string.IsNullOrEmpty(verb) ? null : $"[E]  {verb}";

        public void Interact(PlayerInteractor by) => Activate();

        public void SetFocused(bool value)
        {
            _focused = value;
            ApplyTint();
        }

        public void Activate()
        {
            _flashUntil = Time.time + activateFlashSeconds;
            ApplyTint();
            onActivate?.Invoke();
        }

        private void Update()
        {
            if (_flashUntil > 0f && Time.time > _flashUntil)
            {
                _flashUntil = 0f;
                ApplyTint();
            }
        }

        private void ApplyTint()
        {
            if (_renderer == null)
            {
                return;
            }

            Color c = _baseColor;
            if (_focused) c = focusColor;
            if (_flashUntil > 0f) c = Color.white;

            _mpb.Clear();
            _mpb.SetColor("_BaseColor", c);
            _mpb.SetColor("_Color", c);
            _renderer.SetPropertyBlock(_mpb);
        }
    }
}
