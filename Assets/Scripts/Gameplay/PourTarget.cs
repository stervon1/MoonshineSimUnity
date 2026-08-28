using System;
using UnityEngine;
using UnityEngine.Events;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// Hold-E to empty the carried <see cref="Carryable"/> vessel into this
    /// station — the mash tub, the fermenter. Generic: it just drains the vessel
    /// and reports what came out. A sibling component (e.g. <see cref="MashStation"/>)
    /// adds the meaning via <see cref="GateProvider"/> / <see cref="PromptProvider"/>
    /// and the pour events.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PourTarget : MonoBehaviour, IHoldInteractable
    {
        [Tooltip("Substance this station accepts. Ignored when AcceptAny is set.")]
        [SerializeField] private VesselContents accept = VesselContents.Grain;
        [SerializeField] private bool acceptAny = false;
        [Tooltip("Vessel units drained per second held.")]
        [SerializeField] private float unitsPerSecond = 0.8f;
        [SerializeField] private string verb = "Pour it in";

        [Serializable]
        public class FloatEvent : UnityEvent<float> { }

        /// <summary>Raised the first frame of a pour.</summary>
        public UnityEvent onPourStarted = new UnityEvent();
        /// <summary>Units received this frame.</summary>
        public FloatEvent onPour = new FloatEvent();
        /// <summary>Pour ended — argument is the running <see cref="Received"/> total.</summary>
        public FloatEvent onPourEnded = new FloatEvent();

        /// <summary>Total units poured in since the last <see cref="ResetReceived"/>.</summary>
        public float Received { get; private set; }

        /// <summary>Extra gate a sibling can impose (stage checks etc.). Null = allowed.</summary>
        [NonSerialized] public Func<bool> GateProvider;
        /// <summary>When it returns a non-empty string, it replaces the prompt (e.g. "Mashing… 40%").</summary>
        [NonSerialized] public Func<string> PromptProvider;

        private bool _pouring;

        public void ResetReceived() => Received = 0f;

        private bool Accepts(VesselContents k) => acceptAny || k == accept;

        private static Carryable Vessel(PlayerInteractor by) => by != null ? by.Carried : null;

        public bool CanHold(PlayerInteractor by)
        {
            var c = Vessel(by);
            if (c == null || c.IsEmpty || !Accepts(c.Contents)) return false;
            return GateProvider == null || GateProvider();
        }

        public void HoldTick(PlayerInteractor by, float deltaTime)
        {
            var c = Vessel(by);
            if (c == null) return;
            if (!_pouring) { _pouring = true; onPourStarted?.Invoke(); }
            float got = c.Remove(unitsPerSecond * deltaTime);
            if (got > 0f)
            {
                Received += got;
                onPour?.Invoke(got);
            }
        }

        public void HoldEnded(PlayerInteractor by, bool cancelled)
        {
            if (!_pouring) return;
            _pouring = false;
            onPourEnded?.Invoke(Received);
        }

        public float HoldProgress01(PlayerInteractor by)
        {
            var c = Vessel(by);
            return c != null && c.IsVessel ? c.Fill01 : -1f; // meter drains as you pour
        }

        public string GetPrompt()
        {
            string over = PromptProvider?.Invoke();
            if (!string.IsNullOrEmpty(over)) return over;

            var c = Vessel(PlayerInteractor.Active);
            if (c == null || c.IsEmpty) return null;
            if (!Accepts(c.Contents)) return null;
            if (GateProvider != null && !GateProvider()) return null;
            return $"[Hold E]  {verb}  ({c.Fill01:P0})";
        }

        public void SetFocused(bool focused) { }

        public void Interact(PlayerInteractor by) { }
    }
}
