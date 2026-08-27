using MoonshineSim.Core;
using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// Water-cut station. Interact to add a measure of water, dropping the jar's
    /// proof one step. Watch the physical dial; stop when it's where you want it
    /// for the buyer you have in mind.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ProofingStation : MonoBehaviour, IInteractable
    {
        [SerializeField] private BatchController batch;

        private void Awake() => batch = batch != null ? batch : BatchController.Instance;

        public string GetPrompt()
        {
            if (batch == null) batch = BatchController.Instance;
            if (batch == null || !batch.CanProof) return null;
            return $"[E]  Add water  ({batch.Batch.currentProof:0} proof)";
        }

        public void SetFocused(bool focused) { }

        public void Interact(PlayerInteractor by)
        {
            if (batch == null) batch = BatchController.Instance;
            batch?.AddProofingWater();
        }
    }
}
