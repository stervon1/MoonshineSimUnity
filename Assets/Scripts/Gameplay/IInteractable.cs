namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// Anything the <see cref="PlayerInteractor"/> can look at and activate.
    /// Implemented by white-block <see cref="Interactable"/>s, carryables, and
    /// every workshop station — the game has no screen-space action UI.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Prompt shown while focused; null/empty hides the prompt.</summary>
        string GetPrompt();

        void SetFocused(bool focused);

        void Interact(PlayerInteractor by);
    }
}
