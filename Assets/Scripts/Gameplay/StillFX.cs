using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// Drives placeholder still VFX — a liquid-stream ParticleSystem at the
    /// outlet whose colour/clarity reads <see cref="StillCutPhase"/> + the last
    /// cut quality, and a steam ParticleSystem off the pot that ramps with the
    /// run. Swap the ParticleSystems for a VFX Graph effect later; the driver
    /// stays.
    /// </summary>
    public class StillFX : MonoBehaviour
    {
        [SerializeField] private StillRunController stillRun;
        [SerializeField] private ParticleSystem stream;
        [SerializeField] private ParticleSystem steam;

        [SerializeField] private float streamRate = 45f;
        [SerializeField] private float steamRateIdle = 2f;
        [SerializeField] private float steamRateRun = 16f;

        private bool _running;
        private float _lastCutQuality = 0.6f;

        private void OnEnable()
        {
            if (stillRun == null) stillRun = FindAnyObjectByType<StillRunController>();
            if (stillRun == null) return;
            stillRun.OnRunStarted += HandleStart;
            stillRun.OnRunFinished += HandleFinish;
            stillRun.OnCutMade += HandleCut;
            stillRun.OnProofUpdated += HandleProof;
            Apply(0f);
        }

        private void OnDisable()
        {
            if (stillRun == null) return;
            stillRun.OnRunStarted -= HandleStart;
            stillRun.OnRunFinished -= HandleFinish;
            stillRun.OnCutMade -= HandleCut;
            stillRun.OnProofUpdated -= HandleProof;
        }

        private void HandleStart() { _running = true; Apply(0f); }
        private void HandleFinish(StillRunResult _) { _running = false; Apply(0f); }
        private void HandleCut(string _, float quality) => _lastCutQuality = quality;
        private void HandleProof(float proof) => Apply(proof);

        private void Apply(float proof)
        {
            if (stream != null)
            {
                var main = stream.main;
                main.startColor = ColorForPhase();
                var em = stream.emission;
                em.rateOverTime = _running ? streamRate : 0f;
            }
            if (steam != null)
            {
                var em = steam.emission;
                em.rateOverTime = _running
                    ? Mathf.Lerp(steamRateIdle, steamRateRun, Mathf.Clamp01(proof / 120f))
                    : steamRateIdle;
            }
        }

        private Color ColorForPhase()
        {
            switch (stillRun != null ? stillRun.CurrentPhase : StillCutPhase.Heads)
            {
                case StillCutPhase.Hearts:
                    return Color.Lerp(new Color(0.9f, 0.9f, 0.85f, 0.45f),
                                      new Color(1f, 1f, 1f, 0.9f), _lastCutQuality);
                case StillCutPhase.Tails:
                    return new Color(0.72f, 0.74f, 0.62f, 0.4f);
                default: // Heads / Done
                    return new Color(0.86f, 0.85f, 0.72f, 0.6f);
            }
        }
    }
}
