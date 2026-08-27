using UnityEngine;

namespace MoonshineSim.Core
{
    /// <summary>
    /// Rolls a small roster of back-door buyers. Distinct from
    /// <see cref="BatchPlanGenerator"/>, which rolls what the player makes.
    /// </summary>
    public static class BuyerGenerator
    {
        private static readonly string[] Names =
        {
            "Rosa's Diner", "The Blue Owl", "Hollis BBQ", "Pearl St. Tavern", "Ma Kettle's"
        };

        public static Buyer[] GenerateRoster(int count = 3)
        {
            count = Mathf.Clamp(count, 1, Names.Length);
            var roster = new Buyer[count];
            for (int i = 0; i < count; i++)
            {
                float mid = 82f + Random.value * 28f;   // 82 - 110 proof
                float halfBand = 3f + Random.value * 5f; // +/- 3 - 8
                roster[i] = new Buyer
                {
                    name = Names[i],
                    preferredStyle = (SpiritStyle)Random.Range(0, 6),
                    proofMin = mid - halfBand,
                    proofMax = mid + halfBand,
                    smoothLean = Random.Range(-1f, 1f),
                    priceSensitivity = 0.7f + Random.value * 0.8f,
                    appetiteJars = Random.Range(1, 4),
                    rapport = 0
                };
            }
            return roster;
        }
    }
}
