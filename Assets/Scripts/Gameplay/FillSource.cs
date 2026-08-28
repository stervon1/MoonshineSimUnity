using MoonshineSim.Core;
using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// Hold-E to fill the carried <see cref="Carryable"/> vessel with a
    /// substance — a grain bin, a water tap, the still outlet. Flow runs while
    /// the button is held; releasing stops it. No hard fail: filling with
    /// nothing to hold, or a full/incompatible vessel, just shows a hint.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class FillSource : MonoBehaviour, IHoldInteractable
    {
        [SerializeField] private VesselContents provides = VesselContents.Grain;
        [Tooltip("Vessel units added per second held.")]
        [SerializeField] private float unitsPerSecond = 0.6f;
        [Tooltip("Shown in the prompt, e.g. \"corn\", \"water\".")]
        [SerializeField] private string label = "grain";
        [Tooltip("Only when provides == Grain: the style stamped onto the vessel.")]
        [SerializeField] private SpiritStyle grainStyle = SpiritStyle.CornWhiskey;

        private static Carryable Vessel(PlayerInteractor by) => by != null ? by.Carried : null;

        private bool StyleClash(Carryable c) =>
            provides == VesselContents.Grain && !c.IsEmpty &&
            c.Contents == VesselContents.Grain && c.GrainStyle != grainStyle;

        public bool CanHold(PlayerInteractor by)
        {
            var c = Vessel(by);
            return c != null && c.IsVessel && !c.IsFull && c.CanReceive(provides) && !StyleClash(c);
        }

        public void HoldTick(PlayerInteractor by, float deltaTime)
        {
            var c = Vessel(by);
            if (c == null) return;
            if (provides == VesselContents.Grain && c.IsEmpty) c.SetGrainStyle(grainStyle);
            c.Receive(provides, unitsPerSecond * deltaTime);
        }

        public void HoldEnded(PlayerInteractor by, bool cancelled) { }

        public float HoldProgress01(PlayerInteractor by)
        {
            var c = Vessel(by);
            return c != null && c.IsVessel ? c.Fill01 : -1f;
        }

        public string GetPrompt()
        {
            // Prompt reads the player through the singleton interactor — cheap and
            // there is only ever one.
            var by = PlayerInteractor.Active;
            var c = Vessel(by);

            if (c == null || !c.IsVessel) return $"Bring a vessel to fill with {label}";
            if (StyleClash(c)) return $"Bucket already has {c.GrainStyle} — empty it first";
            if (c.IsFull) return $"{c.Kind} is full  (100%)";
            return $"[Hold E]  Fill with {label}  ({c.Fill01:P0})";
        }

        public void SetFocused(bool focused) { }

        // Tap with no hold available: nothing to do — the prompt already explains.
        public void Interact(PlayerInteractor by) { }
    }
}
