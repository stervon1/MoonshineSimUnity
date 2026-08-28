using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// Audio hooks for the still run. Clip slots are empty by default — drop in
    /// cues in the Inspector. The cut cue's pitch tracks cut quality so a clean
    /// hearts cut reads brighter than a mistimed one.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class StillAudio : MonoBehaviour
    {
        [SerializeField] private StillRunController stillRun;
        [SerializeField] private AudioClip runStartCue;
        [SerializeField] private AudioClip cutCue;
        [SerializeField] private AudioClip runFinishCue;
        [SerializeField] private AudioClip boilLoop;
        [SerializeField] private AudioClip coolingCueClip;

        private AudioSource _source;

        private void Awake() => _source = GetComponent<AudioSource>();

        private void OnEnable()
        {
            if (stillRun == null) stillRun = FindAnyObjectByType<StillRunController>();
            if (stillRun == null) return;
            stillRun.OnRunStarted += HandleStart;
            stillRun.OnCutMade += HandleCut;
            stillRun.OnRunFinished += HandleFinish;
            stillRun.OnCoolingCueChanged += HandleCoolingCue;
        }

        private void OnDisable()
        {
            if (stillRun == null) return;
            stillRun.OnRunStarted -= HandleStart;
            stillRun.OnCutMade -= HandleCut;
            stillRun.OnRunFinished -= HandleFinish;
            stillRun.OnCoolingCueChanged -= HandleCoolingCue;
        }

        private void HandleStart()
        {
            if (runStartCue != null) _source.PlayOneShot(runStartCue);
            if (boilLoop != null)
            {
                _source.clip = boilLoop;
                _source.loop = true;
                _source.Play();
            }
        }

        private void HandleCut(string label, float quality)
        {
            if (cutCue == null) return;
            _source.pitch = Mathf.Lerp(0.8f, 1.25f, Mathf.Clamp01(quality));
            _source.PlayOneShot(cutCue);
        }

        private void HandleFinish(StillRunResult _)
        {
            if (_source.loop) { _source.Stop(); _source.loop = false; _source.clip = null; }
            _source.pitch = 1f;
            if (runFinishCue != null) _source.PlayOneShot(runFinishCue);
        }

        private void HandleCoolingCue(bool needsAttention)
        {
            if (needsAttention && coolingCueClip != null) _source.PlayOneShot(coolingCueClip);
        }
    }
}
