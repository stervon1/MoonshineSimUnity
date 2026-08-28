namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// A world object whose action is performed by <b>holding</b> E (or LMB)
    /// rather than a single press — filling a bucket, pouring it out, watering
    /// down a jar. <see cref="PlayerInteractor"/> calls <see cref="HoldTick"/>
    /// every frame while the button is down and <see cref="CanHold"/> is true,
    /// and <see cref="HoldEnded"/> when the button releases or focus is lost.
    ///
    /// Still an <see cref="IInteractable"/>: <see cref="IInteractable.GetPrompt"/>
    /// should include a "(NN%)" meter while a hold is available, and a plain tap
    /// (when <see cref="CanHold"/> is false) falls through to
    /// <see cref="IInteractable.Interact"/> for a hint / no-op — no hard fail.
    /// </summary>
    public interface IHoldInteractable : IInteractable
    {
        /// <summary>Is a hold action available right now for this interactor?</summary>
        bool CanHold(PlayerInteractor by);

        /// <summary>Called once per frame while the button is held and <see cref="CanHold"/> is true.</summary>
        void HoldTick(PlayerInteractor by, float deltaTime);

        /// <summary>Button released, focus lost, or <see cref="CanHold"/> went false.</summary>
        void HoldEnded(PlayerInteractor by, bool cancelled);

        /// <summary>0..1 for the on-screen meter; return a negative value to hide it.</summary>
        float HoldProgress01(PlayerInteractor by);
    }
}
