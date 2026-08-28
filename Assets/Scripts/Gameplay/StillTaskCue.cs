using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// Pulses an indicator light while <see cref="StillRunController"/> flags an
    /// intermittent task (currently: cooling water) as needing attention — the
    /// "station cue" the tactile-depth backlog calls for. Purely presentational;
    /// the controller owns the actual quality consequence.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class StillTaskCue : MonoBehaviour
    {
        [SerializeField] private StillRunController stillRun;
        [SerializeField] private float pulseSpeed = 6f;
        [SerializeField] private float idleIntensity = 0f;
        [SerializeField] private float urgentIntensity = 1.4f;

        private Light _light;
        private bool _needsAttention;

        private void Awake() => _light = GetComponent<Light>();

        private void OnEnable()
        {
            if (stillRun == null) stillRun = FindAnyObjectByType<StillRunController>();
            if (stillRun != null) stillRun.OnCoolingCueChanged += HandleCue;
            Apply();
        }

        private void OnDisable()
        {
            if (stillRun != null) stillRun.OnCoolingCueChanged -= HandleCue;
        }

        private void HandleCue(bool needsAttention)
        {
            _needsAttention = needsAttention;
            Apply();
        }

        private void Update()
        {
            if (!_needsAttention || _light == null) return;
            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            _light.intensity = Mathf.Lerp(idleIntensity, urgentIntensity, pulse);
        }

        private void Apply()
        {
            if (_light == null) return;
            _light.enabled = _needsAttention;
            _light.intensity = _needsAttention ? urgentIntensity : idleIntensity;
        }
    }
}
