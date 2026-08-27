using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// A world object the player can pick up and carry (the mash-jar / spirit
    /// jar). Look at it → pick up; it parents to the interactor's hold point.
    /// Stations read <c>PlayerInteractor.Carried</c> to know what you're holding.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Carryable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string kind = "jar";

        public string Kind => kind;
        public bool Held { get; private set; }

        private Collider _col;
        private Rigidbody _rb;
        private Vector3 _homePos;
        private Quaternion _homeRot;

        private void Awake()
        {
            _col = GetComponent<Collider>();
            _rb = GetComponent<Rigidbody>();
            _homePos = transform.position;
            _homeRot = transform.rotation;
        }

        public string GetPrompt() => Held ? null : $"[E]  Pick up {kind}";

        public void SetFocused(bool focused) { }

        public void Interact(PlayerInteractor by)
        {
            if (!Held) by.PickUp(this);
        }

        public void OnPickedUp(Transform holdPoint)
        {
            Held = true;
            transform.SetParent(holdPoint, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            if (_col != null) _col.enabled = false;
            if (_rb != null) { _rb.isKinematic = true; _rb.detectCollisions = false; }
        }

        public void OnDropped(Vector3 position)
        {
            Held = false;
            transform.SetParent(null, true);
            transform.position = position;
            if (_col != null) _col.enabled = true;
            if (_rb != null) { _rb.isKinematic = false; _rb.detectCollisions = true; }
        }

        /// <summary>Send it back to where it started, unheld.</summary>
        public void ReturnHome()
        {
            Held = false;
            transform.SetParent(null, true);
            transform.SetPositionAndRotation(_homePos, _homeRot);
            if (_col != null) _col.enabled = true;
            if (_rb != null) { _rb.isKinematic = true; _rb.detectCollisions = true; }
        }
    }
}
