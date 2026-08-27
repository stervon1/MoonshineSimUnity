using System;
using UnityEngine;

namespace MoonshineSim.Core
{
    [Serializable]
    public struct BatchSaleReport
    {
        public float finalProof;
        public float qualityScore;
        public int payout;
        public int rapportGain;
    }

    /// <summary>
    /// Global game state, equivalent to game_state.gd's autoload in the
    /// Godot prototype and UMoonshineGameState in the Unreal port.
    /// Attach to a persistent GameObject (DontDestroyOnLoad) or convert
    /// to a ScriptableObject singleton if you prefer that pattern.
    /// </summary>
    public class GameState : MonoBehaviour
    {
        public static GameState Instance { get; private set; }

        public event Action<BatchPlan> OnBatchStarted;
        public event Action<string> OnBatchStageChanged;
        public event Action<BatchSaleReport> OnBatchSold;
        public event Action<Upgrade> OnUpgradePurchased;

        [SerializeField] private BatchPlan currentBatch;
        public BatchPlan CurrentBatch => currentBatch;

        /// Rapport with the back-door buyer network (design doc v3 §3.3, §6.2).
        public int Rapport { get; private set; }

        /// Spendable cash — the run toward the licence.
        public int Cash { get; private set; }

        /// How many <see cref="UpgradeTrack"/> steps bought so far.
        public int UpgradeLevel { get; private set; }
        public bool IsLicensed => UpgradeLevel >= UpgradeTrack.Steps.Length;

        /// 1 = Basement (pressure cooker), 2 = Pot still, 3 = Column, 4 = Licensed.
        public int WorkshopTier { get; private set; } = 1;

        public bool BuyNextUpgrade()
        {
            if (IsLicensed) return false;
            var next = UpgradeTrack.Steps[UpgradeLevel];
            if (Cash < next.cost) return false;

            Cash -= next.cost;
            UpgradeLevel++;
            if (UpgradeLevel >= 3) WorkshopTier = 2;
            if (UpgradeLevel >= 4) WorkshopTier = 3;
            if (IsLicensed) WorkshopTier = 4;
            OnUpgradePurchased?.Invoke(next);
            return true;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartBatch(BatchPlan plan)
        {
            currentBatch = plan;
            OnBatchStarted?.Invoke(plan);
            OnBatchStageChanged?.Invoke("mash");
        }

        public void AdvanceStage(string stageName)
        {
            OnBatchStageChanged?.Invoke(stageName);
        }

        public void SellBatch(BatchSaleReport report)
        {
            Rapport += report.rapportGain;
            Cash += report.payout;
            OnBatchSold?.Invoke(report);
            currentBatch = default;
        }
    }
}
