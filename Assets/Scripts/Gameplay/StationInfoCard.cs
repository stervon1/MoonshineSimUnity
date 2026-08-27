using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// A world-space "what does this step do" card, hidden by default.
    /// <see cref="PlayerInteractor"/> toggles the focused station's card on
    /// right-click. The card content is built by the stations builder from the
    /// distillation reference.
    /// </summary>
    public class StationInfoCard : MonoBehaviour
    {
        [SerializeField] private GameObject card;   // the world-space canvas to show/hide

        public bool IsOpen => card != null && card.activeSelf;

        private void Awake()
        {
            if (card != null) card.SetActive(false);
        }

        public void Toggle()
        {
            if (card != null) card.SetActive(!card.activeSelf);
        }

        public void Hide()
        {
            if (card != null) card.SetActive(false);
        }
    }
}
