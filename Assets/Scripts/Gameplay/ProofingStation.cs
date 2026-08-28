using MoonshineSim.Core;
using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// Water-cut station. <b>Hold E</b> to trickle water in — the proof falls
    /// continuously while you hold. Watch the physical dial (and the meter) and
    /// stop when it's where you want it for the buyer you have in mind.
    /// Over-shooting below the band just costs a little smoothness — no fail.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ProofingStation : MonoBehaviour, IHoldInteractable
    {
        [SerializeField] private BatchController batch;
        [Tooltip("Proof points removed per second of holding.")]
        [SerializeField] private float proofPerSecond = 9f;
        [Tooltip("Meter spans this proof range (high = full, low = empty).")]
        [SerializeField] private float meterHighProof = 150f;
        [SerializeField] private float meterLowProof = 70f;

        private void Awake() => batch = batch != null ? batch : BatchController.Instance;

        private bool Ready()
        {
            if (batch == null) batch = BatchController.Instance;
            return batch != null && batch.CanProof;
        }

        public bool CanHold(PlayerInteractor by) => Ready();

        public void HoldTick(PlayerInteractor by, float deltaTime)
        {
            if (Ready()) batch.AddProofingWater(proofPerSecond * deltaTime);
        }

        public void HoldEnded(PlayerInteractor by, bool cancelled) { }

        public float HoldProgress01(PlayerInteractor by)
        {
            if (!Ready()) return -1f;
            return Mathf.InverseLerp(meterLowProof, meterHighProof, batch.Batch.currentProof);
        }

        public string GetPrompt()
        {
            if (!Ready()) return null;
            return $"[Hold E]  Add water  ({batch.Batch.currentProof:0} proof)";
        }

        public void SetFocused(bool focused) { }

        // Tap does nothing here — it's a hold action.
        public void Interact(PlayerInteractor by) { }
    }
}
