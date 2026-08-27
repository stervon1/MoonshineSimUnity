using MoonshineSim.Core;
using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>The mash tub. Interact with a grain chosen to start the mash.</summary>
    [RequireComponent(typeof(Collider))]
    public class MashStation : MonoBehaviour, IInteractable
    {
        [SerializeField] private BatchController batch;

        private void Awake() => batch = batch != null ? batch : BatchController.Instance;

        public string GetPrompt()
        {
            if (batch == null) batch = BatchController.Instance;
            if (batch == null) return null;
            return batch.Batch.stage switch
            {
                BatchStage.GrainChosen => "[E]  Start the mash",
                BatchStage.Mashing => $"Mashing…  {batch.StageProgress01:P0}",
                _ => null
            };
        }

        public void SetFocused(bool focused) { }

        public void Interact(PlayerInteractor by)
        {
            if (batch == null) batch = BatchController.Instance;
            batch?.StartMash();
        }
    }
}
