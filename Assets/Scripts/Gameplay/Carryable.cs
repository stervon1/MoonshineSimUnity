using MoonshineSim.Core;
using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>What a vessel is currently holding. <see cref="Empty"/> = nothing.</summary>
    public enum VesselContents
    {
        Empty,
        Grain,
        Water,
        Wash,
        LowWines,
        Hearts
    }

    /// <summary>
    /// A world object the player can pick up and carry (the bucket, the spirit
    /// jar). Look at it → pick up; it parents to the interactor's hold point.
    /// Stations read <c>PlayerInteractor.Carried</c> to know what you're holding.
    ///
    /// When <see cref="Capacity"/> &gt; 0 it is also a <b>vessel</b>: a
    /// <see cref="FillSource"/> pours substance in over time and a
    /// <see cref="PourTarget"/> drains it out, both driving <see cref="Fill01"/>
    /// for the on-screen meter and the greybox <see cref="fillVisual"/>.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Carryable : MonoBehaviour, IInteractable
    {
        [SerializeField] private string kind = "jar";

        [Header("Vessel (0 capacity = not fillable)")]
        [SerializeField] private float capacity = 0f;
        [Tooltip("Child mesh scaled on Y to show the fill level. Optional.")]
        [SerializeField] private Transform fillVisual;

        public string Kind => kind;
        public bool Held { get; private set; }

        public float Capacity => capacity;
        public bool IsVessel => capacity > 0f;
        public VesselContents Contents { get; private set; } = VesselContents.Empty;
        public float FillLevel { get; private set; }
        public float Fill01 => capacity > 0f ? Mathf.Clamp01(FillLevel / capacity) : 0f;
        public bool IsEmpty => FillLevel <= 0.0001f;
        public bool IsFull => capacity > 0f && FillLevel >= capacity - 0.0001f;

        /// <summary>Meaningful only while <see cref="Contents"/> is <see cref="VesselContents.Grain"/>.</summary>
        public SpiritStyle GrainStyle { get; private set; }

        private Collider _col;
        private Rigidbody _rb;
        private Vector3 _homePos;
        private Quaternion _homeRot;
        private Vector3 _visFullScale;
        private float _visBaseY;

        private void Awake()
        {
            _col = GetComponent<Collider>();
            _rb = GetComponent<Rigidbody>();
            _homePos = transform.position;
            _homeRot = transform.rotation;

            if (fillVisual != null)
            {
                _visFullScale = fillVisual.localScale;
                _visBaseY = fillVisual.localPosition.y - _visFullScale.y * 0.5f; // bottom of the mesh
            }
            UpdateFillVisual();
        }

        // --- vessel API ---------------------------------------------------

        /// <summary>Can this vessel take <paramref name="kind"/> right now?</summary>
        public bool CanReceive(VesselContents kind) =>
            capacity > 0f && !IsFull && (IsEmpty || Contents == kind);

        /// <summary>Pour <paramref name="amount"/> of <paramref name="kind"/> in; returns the overflow that didn't fit.</summary>
        public float Receive(VesselContents kind, float amount)
        {
            if (amount <= 0f || !CanReceive(kind)) return amount;
            if (IsEmpty) Contents = kind;
            float take = Mathf.Min(capacity - FillLevel, amount);
            FillLevel += take;
            UpdateFillVisual();
            return amount - take;
        }

        /// <summary>Draw <paramref name="amount"/> out; returns how much was actually removed.</summary>
        public float Remove(float amount)
        {
            if (amount <= 0f || IsEmpty) return 0f;
            float give = Mathf.Min(FillLevel, amount);
            FillLevel -= give;
            if (IsEmpty) Contents = VesselContents.Empty;
            UpdateFillVisual();
            return give;
        }

        public void SetGrainStyle(SpiritStyle style) => GrainStyle = style;

        public void EmptyOut()
        {
            FillLevel = 0f;
            Contents = VesselContents.Empty;
            UpdateFillVisual();
        }

        private void UpdateFillVisual()
        {
            if (fillVisual == null) return;
            float f = Fill01;
            fillVisual.gameObject.SetActive(f > 0.001f);
            var s = _visFullScale;
            s.y = Mathf.Max(0.0001f, _visFullScale.y * f);
            fillVisual.localScale = s;
            var p = fillVisual.localPosition;
            p.y = _visBaseY + s.y * 0.5f;
            fillVisual.localPosition = p;
        }

        // --- carry ------------------------------------------------------

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

        /// <summary>Send it back to where it started, unheld. Does not change contents.</summary>
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
