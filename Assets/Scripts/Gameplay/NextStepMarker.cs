using MoonshineSim.Core;
using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// A bobbing marker that hovers over whichever station the current
    /// <see cref="BatchStage"/> says to go to next. Purely a guide — hide it
    /// once the loop is learned.
    /// </summary>
    public class NextStepMarker : MonoBehaviour
    {
        [SerializeField] private BatchController batch;
        [SerializeField] private Transform arrow;          // visible mesh (bobs + spins)

        [Header("Targets")]
        [SerializeField] private Transform grainTarget;
        [SerializeField] private Transform mashTarget;
        [SerializeField] private Transform fermentTarget;
        [SerializeField] private Transform stillTarget;
        [SerializeField] private Transform proofTarget;
        [SerializeField] private Transform sellTarget;

        [Header("Motion")]
        [SerializeField] private float hoverHeight = 1.7f;
        [SerializeField] private float bobAmplitude = 0.15f;
        [SerializeField] private float bobSpeed = 2.5f;
        [SerializeField] private float spinSpeed = 55f;

        private Transform _target;

        private void OnEnable()
        {
            if (batch == null) batch = BatchController.Instance;
            if (batch != null)
            {
                batch.OnStageChanged += HandleStage;
                HandleStage(batch.Batch.stage);
            }
        }

        private void OnDisable()
        {
            if (batch != null) batch.OnStageChanged -= HandleStage;
        }

        private void HandleStage(BatchStage s)
        {
            _target = s switch
            {
                BatchStage.None or BatchStage.Sold => grainTarget,
                BatchStage.GrainChosen or BatchStage.Mashing => mashTarget,
                BatchStage.Mashed or BatchStage.Fermenting => fermentTarget,
                BatchStage.WashReady or BatchStage.Distilling => stillTarget,
                BatchStage.Distilled => proofTarget,
                BatchStage.Proofed => sellTarget,
                _ => null
            };
        }

        private void Update()
        {
            bool show = _target != null;
            if (arrow != null && arrow.gameObject.activeSelf != show)
            {
                arrow.gameObject.SetActive(show);
            }
            if (!show) return;

            Vector3 basePos = _target.position + Vector3.up * hoverHeight;
            transform.position = basePos + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobAmplitude);
            if (arrow != null) arrow.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        }
    }
}
