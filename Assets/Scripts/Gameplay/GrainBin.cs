using MoonshineSim.Core;
using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// A grain bin. Taking from it chooses the spirit style and starts a fresh
    /// batch — this is the whole "batch plan" step, no UI.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class GrainBin : MonoBehaviour, IInteractable
    {
        [SerializeField] private BatchController batch;
        [SerializeField] private SpiritStyle style = SpiritStyle.CornWhiskey;

        private void Awake() => batch = batch != null ? batch : BatchController.Instance;

        public string GetPrompt()
        {
            if (batch == null) batch = BatchController.Instance;
            return batch != null && batch.CanChooseGrain ? $"[E]  Take {Label}" : null;
        }

        public void SetFocused(bool focused) { }

        public void Interact(PlayerInteractor by)
        {
            if (batch == null) batch = BatchController.Instance;
            batch?.ChooseGrain(style);
        }

        private string Label => style switch
        {
            SpiritStyle.CornWhiskey => "corn",
            SpiritStyle.Rye => "rye",
            SpiritStyle.SugarShine => "sugar",
            SpiritStyle.Wheat => "wheat",
            SpiritStyle.MaltedBarley => "malt",
            SpiritStyle.Molasses => "molasses",
            _ => style.ToString()
        };
    }
}
