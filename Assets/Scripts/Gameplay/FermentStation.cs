using MoonshineSim.Core;
using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>The fermenter. Interact with a finished mash to pitch the yeast
    /// and time-skip fermentation.</summary>
    [RequireComponent(typeof(Collider))]
    public class FermentStation : MonoBehaviour, IInteractable
    {
        [SerializeField] private BatchController batch;

        private void Awake() => batch = batch != null ? batch : BatchController.Instance;

        public string GetPrompt()
        {
            if (batch == null) batch = BatchController.Instance;
            if (batch == null) return null;
            return batch.Batch.stage switch
            {
                BatchStage.Mashed => "[E]  Pitch the yeast",
                BatchStage.Fermenting => $"Fermenting…  {batch.StageProgress01:P0}",
                BatchStage.WashReady => "Wash ready — take it to the still",
                _ => null
            };
        }

        public void SetFocused(bool focused) { }

        public void Interact(PlayerInteractor by)
        {
            if (batch == null) batch = BatchController.Instance;
            batch?.StartFerment();
        }
    }
}
