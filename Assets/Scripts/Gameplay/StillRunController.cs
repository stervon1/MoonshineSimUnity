using System;
using UnityEngine;

namespace MoonshineSim.Gameplay
{
    public enum StillCutPhase
    {
        Heads,
        Hearts,
        Tails,
        Done
    }

    [Serializable]
    public struct StillRunResult
    {
        public float heartsVolume;
        public float averageQuality;
        public int equipmentTier;
    }

    /// <summary>
    /// The still-run centerpiece interaction (design doc section 4.3).
    ///
    /// Run it like a real pot still: watch the **vapour temperature** climb —
    /// it warms to ~78 C (ethanol's boil), plateaus through the hearts, then
    /// heads for the 90s as the boiler runs dry. Cut to hearts as it steadies,
    /// cut to tails as it climbs away. A jury-rigged **pressure-cooker** rig
    /// (tier 1) also builds pressure — vent it before it redlines or the run
    /// gets rougher. No hard fail: mistiming just lowers the quality score.
    /// </summary>
    public class StillRunController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private float runDurationSeconds = 60f;

        [Tooltip("1 = pressure cooker, 2 = pot still, 3 = column still.")]
        [SerializeField] private int equipmentTier = 1;

        [Tooltip("Optional: tuned proof curve. If unset, falls back to a sine curve.")]
        [SerializeField] private AnimationCurve proofCurve;

        [SerializeField] private float idealHeadsEnd = 0.15f;
        [SerializeField] private float idealTailsStart = 0.75f;

        [Header("Temperature (deg C)")]
        [SerializeField] private float ambientTempC = 20f;
        [SerializeField] private float heartsPlateauTempC = 78f;
        [SerializeField] private float spentTempC = 96f;
        [SerializeField, Range(0.02f, 0.3f)] private float warmupFraction = 0.08f;

        [Header("Pressure (psi, tier 1 only)")]
        [SerializeField] private float pressureStartPsi = 3f;
        [SerializeField] private float pressureRisePerSecond = 0.16f;
        [SerializeField] private float pressureRedlinePsi = 14f;
        [SerializeField] private float ventReliefPsi = 9f;

        public event Action OnRunStarted;
        public event Action<string, float> OnCutMade;          // (phaseLabel, qualityAtCut)
        public event Action<StillRunResult> OnRunFinished;
        public event Action<float> OnProofUpdated;             // proof (0..~150)
        public event Action<float> OnTemperatureUpdated;       // deg C
        public event Action<float> OnPressureUpdated;          // psi

        public StillCutPhase CurrentPhase { get; private set; } = StillCutPhase.Heads;
        public float CurrentTemperatureC { get; private set; }
        public float CurrentPressurePsi { get; private set; }
        public bool Running => _running;
        public float RunProgress01 => _running ? Mathf.Clamp01(_elapsed / runDurationSeconds) : 0f;

        private float _elapsed;
        private float _heartsVolume;
        private float _heartsQualitySum;
        private int _heartsSamples;
        private float _pressure;
        private bool _running;

        public void StartRun()
        {
            _elapsed = 0f;
            CurrentPhase = StillCutPhase.Heads;
            _heartsVolume = 0f;
            _heartsQualitySum = 0f;
            _heartsSamples = 0;
            _pressure = pressureStartPsi;
            _running = true;
            OnRunStarted?.Invoke();
        }

        /// <summary>Bleed the pressure-cooker rig (tier 1). Safe to call anytime.</summary>
        public void Vent()
        {
            _pressure = Mathf.Max(pressureStartPsi, _pressure - ventReliefPsi);
        }

        private void Update()
        {
            if (!_running) return;

            _elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(_elapsed / runDurationSeconds);

            // Tier-1 rigs show a slow needle sway (smooth noise), not jitter.
            float sway = equipmentTier == 1
                ? (Mathf.PerlinNoise(_elapsed * 1.4f, 0.37f) - 0.5f) * 0.03f
                : 0f;

            float currentProof = EvaluateProofCurve(Mathf.Clamp01(progress + sway));
            OnProofUpdated?.Invoke(currentProof);

            CurrentTemperatureC = EvaluateTemperature(progress)
                                  + (equipmentTier == 1 ? sway * 25f : 0f);
            OnTemperatureUpdated?.Invoke(CurrentTemperatureC);

            if (equipmentTier == 1)
            {
                _pressure += pressureRisePerSecond * Time.deltaTime;
            }
            CurrentPressurePsi = _pressure + (equipmentTier == 1 ? sway * 20f : 0f);
            OnPressureUpdated?.Invoke(CurrentPressurePsi);

            if (CurrentPhase == StillCutPhase.Hearts)
            {
                _heartsVolume += Time.deltaTime;
                float q = CutQuality(progress);
                if (_pressure > pressureRedlinePsi) q *= 0.6f;   // running hot
                _heartsQualitySum += q;
                _heartsSamples++;
            }

            if (_elapsed >= runDurationSeconds)
            {
                FinishRun();
            }
        }

        private float EvaluateProofCurve(float progress)
        {
            float p = Mathf.Clamp01(progress);
            if (proofCurve != null && proofCurve.length > 0)
            {
                return proofCurve.Evaluate(p) * 100f;
            }
            return 100f * Mathf.Sin(Mathf.PI * p);
        }

        private float EvaluateTemperature(float progress)
        {
            if (progress < warmupFraction)
            {
                return Mathf.Lerp(ambientTempC, heartsPlateauTempC, progress / warmupFraction);
            }
            float t = (progress - warmupFraction) / Mathf.Max(0.0001f, 1f - warmupFraction);
            // ease so it lingers near the plateau then climbs late in the run
            return Mathf.Lerp(heartsPlateauTempC, spentTempC, t * t);
        }

        private float CutQuality(float progress)
        {
            float windowCenter = (idealHeadsEnd + idealTailsStart) / 2f;
            float windowHalfWidth = (idealTailsStart - idealHeadsEnd) / 2f;
            float distance = Mathf.Abs(progress - windowCenter);
            return Mathf.Clamp01(1f - (distance / windowHalfWidth));
        }

        public void CallCutToHearts()
        {
            if (CurrentPhase != StillCutPhase.Heads) return;
            CurrentPhase = StillCutPhase.Hearts;
            float progress = Mathf.Clamp01(_elapsed / runDurationSeconds);
            OnCutMade?.Invoke("hearts_started", CutQuality(progress));
        }

        public void CallCutToTails()
        {
            if (CurrentPhase != StillCutPhase.Hearts) return;
            CurrentPhase = StillCutPhase.Tails;
            OnCutMade?.Invoke("tails_started", 0f);
        }

        private void FinishRun()
        {
            _running = false;
            CurrentPhase = StillCutPhase.Done;

            float averageQuality = _heartsSamples > 0 ? _heartsQualitySum / _heartsSamples : 0f;

            OnRunFinished?.Invoke(new StillRunResult
            {
                heartsVolume = _heartsVolume,
                averageQuality = averageQuality,
                equipmentTier = equipmentTier
            });
        }
    }
}
