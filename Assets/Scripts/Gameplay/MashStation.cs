using MoonshineSim.Core;
using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// The mash tub. Carry a bucket of grain over and <b>hold E to pour it in</b>
    /// (a sibling <see cref="PourTarget"/> handles the pour); the first splash
    /// picks the spirit style, and how much you pour sets the batch size —
    /// capped by the rig. Release with enough grain in and the mash starts.
    /// Under-fill = a smaller batch, never a failure.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(PourTarget))]
    public class MashStation : MonoBehaviour
    {
        [SerializeField] private BatchController batch;
        [Tooltip("Minimum vessel units poured in before the mash will start.")]
        [SerializeField] private float minGrainToMash = 0.35f;
        [Tooltip("Gallons of batch per vessel unit of grain poured.")]
        [SerializeField] private float gallonsPerGrainUnit = 6f;

        private PourTarget _pour;

        private void Awake()
        {
            if (batch == null) batch = BatchController.Instance;
            _pour = GetComponent<PourTarget>();
            _pour.GateProvider = CanReceiveGrain;
            _pour.PromptProvider = StagePrompt;
            _pour.onPourStarted.AddListener(HandlePourStarted);
            _pour.onPourEnded.AddListener(HandlePourEnded);
        }

        private void OnDestroy()
        {
            if (_pour == null) return;
            _pour.onPourStarted.RemoveListener(HandlePourStarted);
            _pour.onPourEnded.RemoveListener(HandlePourEnded);
        }

        private bool Ready()
        {
            if (batch == null) batch = BatchController.Instance;
            return batch != null;
        }

        /// <summary>Pouring grain is allowed until the mash has actually started.</summary>
        private bool CanReceiveGrain()
        {
            if (!Ready()) return false;
            var s = batch.Batch.stage;
            return s == BatchStage.None || s == BatchStage.GrainChosen || s == BatchStage.Sold;
        }

        private string StagePrompt()
        {
            if (!Ready()) return null;
            return batch.Batch.stage switch
            {
                BatchStage.Mashing => $"Mashing…  {batch.StageProgress01:P0}",
                _ => null // let PourTarget show its own "[Hold E] Pour it in (NN%)"
            };
        }

        private void HandlePourStarted()
        {
            if (!Ready() || !CanReceiveGrain()) return;

            // A fresh batch (stage None/Sold): pick the style and zero the tally.
            // Topping up an existing GrainChosen batch keeps accumulating.
            if (batch.CanChooseGrain)
            {
                var carried = PlayerInteractor.Active != null ? PlayerInteractor.Active.Carried : null;
                var style = carried != null ? carried.GrainStyle : SpiritStyle.CornWhiskey;
                batch.ChooseGrain(style);
                _pour.ResetReceived();
            }
        }

        private void HandlePourEnded(float totalPoured)
        {
            if (!Ready()) return;

            // Not enough grain in the tub yet — keep waiting, no penalty. The
            // tally is cumulative, so another bucket picks up where this left off.
            if (totalPoured < minGrainToMash) return;

            batch.SetBatchSize(totalPoured * gallonsPerGrainUnit); // clamped to the rig cap
            batch.StartMash();
        }
    }
}
