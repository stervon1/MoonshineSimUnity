using MoonshineSim.Core;
using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// A back door. Carry a finished jar here and interact — the buyer appraises
    /// it against their taste and pays. Their standing preference is on the
    /// clipboard; you chose to come to this one.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class BuyerCounter : MonoBehaviour, IInteractable
    {
        [SerializeField] private BatchController batch;
        [SerializeField] private int buyerIndex;

        private void Awake() => batch = batch != null ? batch : BatchController.Instance;

        private Buyer Buyer
        {
            get
            {
                if (batch == null) batch = BatchController.Instance;
                if (batch == null || batch.Buyers == null || batch.Buyers.Length == 0) return null;
                return batch.Buyers[Mathf.Clamp(buyerIndex, 0, batch.Buyers.Length - 1)];
            }
        }

        public string GetPrompt()
        {
            var b = Buyer;
            if (b == null || batch == null) return null;

            string lean = b.smoothLean > 0.2f ? "smooth" : b.smoothLean < -0.2f ? "bold" : "either";
            string wants = $"{b.name} wants {Spirits.DrinkName(b.preferredStyle)}, {b.proofMin:0}-{b.proofMax:0} proof, {lean}";
            return batch.CanSell ? $"[E]  Sell to {b.name}   ({wants})" : wants;
        }

        public void SetFocused(bool focused) { }

        public void Interact(PlayerInteractor by)
        {
            var b = Buyer;
            if (b == null || batch == null || !batch.CanSell) return;
            batch.SellTo(b);
            by.ClearCarried(returnHome: true);
        }
    }
}
