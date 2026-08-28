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
        [Tooltip("Batch size a fresh grain choice starts at, before you pour grain in.")]
        [SerializeField] private float defaultBatchGallons = 3f;

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
            Batch = new BatchState
            {
                style = style,
                stage = BatchStage.GrainChosen,
                batchSizeGallons = defaultBatchGallons
            };
            ResetJar();
            gameState?.StartBatch(new BatchPlan
            {
                spiritStyle = style,
                quantityGallons = defaultBatchGallons,
                targetProof = 100f,
                proofTolerance = 8f
            });
            OnStageChanged?.Invoke(BatchStage.GrainChosen);
        }

        // ---- mash tub -------------------------------------------------
        public bool CanMash => Batch.stage == BatchStage.GrainChosen;

        /// <summary>Gallons the mash tub will take at a given rig tier.</summary>
        public static float MashCapForTier(int tier) => tier switch
        {
            <= 1 => 3f,
            2 => 6f,
            3 => 12f,
            _ => 20f
        };

        /// <summary>
        /// Set from how much grain was poured into the mash tub, clamped to what
        /// the current rig can hold. Only meaningful before the mash starts.
        /// </summary>
        public void SetBatchSize(float gallons)
        {
            float cap = MashCapForTier(gameState != null ? gameState.WorkshopTier : 1);
            Batch.batchSizeGallons = Mathf.Clamp(gallons, 0.5f, cap);
        }

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

            // Bigger batch → more hearts kept (kept loose; real yield math is M3).
            float sizeFactor = Mathf.Clamp(Batch.batchSizeGallons / Mathf.Max(0.5f, defaultBatchGallons), 0.3f, 4f);

            Batch.heartsQuality = result.averageQuality;
            Batch.heartsVolumeL = Mathf.Max(0.25f, result.heartsVolume * 0.05f) * sizeFactor;
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

        /// <summary>One tap = one measure of water (kept for any press-style callers).</summary>
        public void AddProofingWater() => AddProofingWater(proofWaterStep);

        /// <summary>
        /// Trickle water in: drops the proof by <paramref name="proofDrop"/> points.
        /// Called every frame by <see cref="ProofingStation"/> while E is held.
        /// </summary>
        public void AddProofingWater(float proofDrop)
        {
            if (!CanProof || proofDrop <= 0f) return;
            Batch.currentProof = Mathf.Max(0f, Batch.currentProof - proofDrop);
            // Smoothness gain scaled to the amount added, at the same rate as before.
            Batch.smoothness = Mathf.Clamp01(Batch.smoothness + 0.02f * (proofDrop / Mathf.Max(0.01f, proofWaterStep)));
            if (Batch.stage != BatchStage.Proofed) SetStage(BatchStage.Proofed); // fire the stage event once, not every frame
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
