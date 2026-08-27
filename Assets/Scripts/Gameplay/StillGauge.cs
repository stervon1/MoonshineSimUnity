using UnityEngine;

namespace MoonshineSim.Gameplay
{
    public enum GaugeReading { Proof, Temperature, Pressure }

    /// <summary>
    /// World-space dial. One <see cref="reading"/> per gauge — put a Proof, a
    /// Temperature and (tier 1) a Pressure dial on the still and read them like
    /// a real run. Replaces the old screen proof label.
    /// </summary>
    public class StillGauge : MonoBehaviour
    {
        [SerializeField] private Transform needle;
        [SerializeField] private StillRunController stillRun;
        [SerializeField] private BatchController batch;
        [SerializeField] private GaugeReading reading = GaugeReading.Proof;
        [SerializeField] private float maxValue = 160f;
        [SerializeField] private float sweepDegrees = 240f;
        [SerializeField] private float smoothing = 6f;
        [SerializeField] private Vector3 needleAxis = Vector3.forward;

        private float _target;
        private float _shown;

        private void OnEnable()
        {
            if (stillRun == null) stillRun = FindAnyObjectByType<StillRunController>();
            if (batch == null) batch = BatchController.Instance;

            switch (reading)
            {
                case GaugeReading.Proof:
                    if (stillRun != null) stillRun.OnProofUpdated += Set;
                    if (batch != null) batch.OnProofChanged += Set;
                    break;
                case GaugeReading.Temperature:
                    if (stillRun != null) stillRun.OnTemperatureUpdated += Set;
                    break;
                case GaugeReading.Pressure:
                    if (stillRun != null) stillRun.OnPressureUpdated += Set;
                    break;
            }
        }

        private void OnDisable()
        {
            if (stillRun != null)
            {
                stillRun.OnProofUpdated -= Set;
                stillRun.OnTemperatureUpdated -= Set;
                stillRun.OnPressureUpdated -= Set;
            }
            if (batch != null) batch.OnProofChanged -= Set;
        }

        private void Set(float v) => _target = v;

        private void Update()
        {
            _shown = Mathf.Lerp(_shown, _target, 1f - Mathf.Exp(-smoothing * Time.deltaTime));
            if (needle == null) return;
            float t = Mathf.Clamp01(_shown / Mathf.Max(1f, maxValue));
            float angle = Mathf.Lerp(sweepDegrees * 0.5f, -sweepDegrees * 0.5f, t);
            needle.localRotation = Quaternion.AngleAxis(angle, needleAxis);
        }
    }
}
