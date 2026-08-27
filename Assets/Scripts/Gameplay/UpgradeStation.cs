using MoonshineSim.Core;
using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// The workbench. Interact to buy the next thing on the come-up track with
    /// cash you've earned selling jars — up to the licence. No shop UI: the
    /// prompt says what's next and what it costs; the clipboard tracks progress.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class UpgradeStation : MonoBehaviour, IInteractable
    {
        public string GetPrompt()
        {
            var gs = GameState.Instance;
            if (gs == null) return null;
            if (gs.IsLicensed) return "Licensed. You made it.";

            var next = UpgradeTrack.Steps[gs.UpgradeLevel];
            return gs.Cash >= next.cost
                ? $"[E]  Buy {next.name} - ${next.cost}   ({next.blurb})"
                : $"Next: {next.name} - ${next.cost}   (you have ${gs.Cash})";
        }

        public void SetFocused(bool focused) { }

        public void Interact(PlayerInteractor by) => GameState.Instance?.BuyNextUpgrade();
    }
}
