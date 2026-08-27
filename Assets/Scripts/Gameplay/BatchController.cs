using System;
using MoonshineSim.Core;
using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// Drives one <see cref="BatchState"/> through the workshop from world-station
    /// interactions and bridges to <see cref="GameState"/>. The clipboard listens
    /// to its events. No UI here.
    /// </summary>
    public class BatchController : MonoBehaviour
    {
        [SerializeField] private GameState gameState;
        [SerializeField] private StillRunController stillRun;
        [Tooltip("The Carryable jar near the still — shown once a run finishes.")]
        [SerializeField] private GameObject jar;
        [SerializeField] private float mashSeconds = 2f;
        [SerializeField] private float fermentSeconds = 4f;
        [SerializeField] private float proofWaterStep = 6f;

        public static BatchController Instance { get; private set; }

        public BatchState Batch { get; private set; } = new BatchState();
        public Buyer[] Buyers { get; private set; } = Array.Empty<Buyer>();

        public event Action<BatchStage> OnStageChanged;
        public event Action<float> OnProofChanged;
        public event Action<Buyer, Appraisal> OnSold;

        private Carryable _jarCarryable;
        private float _timerElapsed;
        private float _timerDuration;
        private bool _timerRunning;

        /// <summary>0..1 progress of the current timed stage (mash / ferment); 0 otherwise.</summary>
        public float StageProgress01 =>
            _timerRunning && _timerDuration > 0f ? Mathf.Clamp01(_timerElapsed / _timerDuration) : 0f;

        private void Awake()
        {
            Instance = this;
            if (gameState == null) gameState = GameState.Instance;
            if (jar != null)
            {
                _jarCarryable = jar.GetComponent<Carryable>();
                jar.SetActive(false);
            }
            Buyers = BuyerGenerator.GenerateRoster(3);
        }

        private void OnEnable()
        {
            if (stillRun == null) return;
            stillRun.OnRunStarted += HandleRunStarted;
            stillRun.OnProofUpdated += HandleStillProof;
            stillRun.OnRunFinished += HandleRunFinished;
        }

        private void OnDisable()
        {
            if (stillRun == null) return;
            stillRun.OnRunStarted -= HandleRunStarted;
            stillRun.OnProofUpdated -= HandleStillProof;
            stillRun.OnRunFinished -= HandleRunFinished;
        }

        private void Update()
        {
            if (!_timerRunning) return;
            _timerElapsed += Time.deltaTime;
            if (_timerElapsed < _timerDuration) return;

            _timerRunning = false;
            if (Batch.stage == BatchStage.Mashing) FinishMash();
            else if (Batch.stage == BatchStage.Fermenting) FinishFerment();
        }

        private void StartTimer(float seconds)
        {
            _timerDuration = seconds;
            _timerElapsed = 0f;
            _timerRunning = true;
        }

        private void SetStage(BatchStage s)
        {
            Batch.stage = s;
            OnStageChanged?.Invoke(s);
        }

        // ---- grain bin ---------------------------------------------------
        public bool CanChooseGrain =>
            Batch.stage == BatchStage.None || Batch.stage == BatchStage.Sold;

        public void ChooseGrain(SpiritStyle style)
        {
            if (!CanChooseGrain) return;
            Batch = new BatchState { style = style, stage = BatchStage.GrainChosen };
            ResetJar();
            gameState?.StartBatch(new BatchPlan
            {
                spiritStyle = style,
                quantityGallons = 3f,
                targetProof = 100f,
                proofTolerance = 8f
            });
            OnStageChanged?.Invoke(BatchStage.GrainChosen);
        }

        // ---- mash tub -------------------------------------------------
        public bool CanMash => Batch.stage == BatchStage.GrainChosen;

        public void StartMash()
        {
            if (!CanMash) return;
            SetStage(BatchStage.Mashing);
            StartTimer(mashSeconds);
        }

        private void FinishMash() => SetStage(BatchStage.Mashed);

        // ---- fermenter ---------------------------------------------
        public bool CanFerment => Batch.stage == BatchStage.Mashed;

        public void StartFerment()
        {
            if (!CanFerment) return;
            gameState?.AdvanceStage("ferment");
            SetStage(BatchStage.Fermenting);
            StartTimer(fermentSeconds);
        }

        private void FinishFerment()
        {
            Batch.washAbv = 8f + UnityEngine.Random.value * 4f; // 8-12 %
            gameState?.AdvanceStage("distill");
            SetStage(BatchStage.WashReady);
        }

        // ---- still (driven by StillRunController events) --------
        private void HandleRunStarted()
        {
            if (Batch.stage == BatchStage.WashReady)
            {
                SetStage(BatchStage.Distilling);
            }
        }

        private void HandleStillProof(float proof)
        {
            if (Batch.stage != BatchStage.Distilling) return;
            Batch.currentProof = proof;
            OnProofChanged?.Invoke(proof);
        }

        private void HandleRunFinished(StillRunResult result)
        {
            if (Batch.stage != BatchStage.Distilling) return;

            Batch.heartsQuality = result.averageQuality;
            Batch.heartsVolumeL = Mathf.Max(0.25f, result.heartsVolume * 0.05f);
            Batch.smoothness = Mathf.Clamp01(result.averageQuality);
            Batch.currentProof = Mathf.Lerp(115f, 150f, result.averageQuality);

            ShowJar();
            gameState?.AdvanceStage("proof");
            SetStage(BatchStage.Distilled);
            OnProofChanged?.Invoke(Batch.currentProof);
        }

        // ---- proofing station -------------------------------
        public bool CanProof =>
            Batch.stage == BatchStage.Distilled || Batch.stage == BatchStage.Proofed;

        public void AddProofingWater()
        {
            if (!CanProof) return;
            Batch.currentProof = Mathf.Max(0f, Batch.currentProof - proofWaterStep);
            Batch.smoothness = Mathf.Clamp01(Batch.smoothness + 0.02f);
            SetStage(BatchStage.Proofed);
            OnProofChanged?.Invoke(Batch.currentProof);
        }

        // ---- buyer counter --------------------------------
        public bool CanSell =>
            Batch.stage == BatchStage.Proofed || Batch.stage == BatchStage.Distilled;

        public Appraisal SellTo(Buyer buyer)
        {
            var appraisal = BatchAppraisal.Appraise(Batch, buyer);
            if (buyer == null || !CanSell) return appraisal;

            buyer.rapport = Mathf.Clamp(buyer.rapport + appraisal.rapportGain, 0, 100);
            gameState?.SellBatch(new BatchSaleReport
            {
                finalProof = Batch.currentProof,
                qualityScore = appraisal.matchScore,
                payout = appraisal.price,
                rapportGain = appraisal.rapportGain
            });

            ResetJar();
            SetStage(BatchStage.Sold);
            OnSold?.Invoke(buyer, appraisal);
            return appraisal;
        }

        // ---- jar helpers ---------------------------------
        private void ShowJar()
        {
            if (jar == null) return;
            jar.SetActive(true);
            if (_jarCarryable != null) _jarCarryable.ReturnHome();
        }

        private void ResetJar()
        {
            if (_jarCarryable != null) _jarCarryable.ReturnHome();
            if (jar != null) jar.SetActive(false);
        }
    }
}
