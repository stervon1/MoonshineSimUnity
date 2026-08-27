using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// Lives on the still body. Makes it look-at-focusable (so right-click opens
    /// its info card) and, while a run is going, shows live status in the
    /// prompt: phase, % complete, temperature, pressure. Idle = no prompt.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class StillStatusReadout : MonoBehaviour, IInteractable
    {
        [SerializeField] private StillRunController stillRun;

        private void Awake()
        {
            if (stillRun == null) stillRun = GetComponent<StillRunController>();
            if (stillRun == null) stillRun = FindAnyObjectByType<StillRunController>();
        }

        public string GetPrompt()
        {
            if (stillRun == null || !stillRun.Running) return null;
            return $"{stillRun.CurrentPhase} - {stillRun.RunProgress01:P0}   " +
                   $"{stillRun.CurrentTemperatureC:0} C   {stillRun.CurrentPressurePsi:0} psi";
        }

        public void SetFocused(bool focused) { }

        public void Interact(PlayerInteractor by) { }
    }
}
