using UnityEngine;

namespace MoonshineSim.Core
{
    public enum SpiritStyle
    {
        CornWhiskey,
        Rye,
        SugarShine,
        Wheat,
        MaltedBarley,
        Molasses
    }

    public enum FlavorRequest
    {
        None,
        Smoother,
        HeavierCornSweetness,
        Spicier,
        CleanAndNeutral
    }

    public static class Spirits
    {
        /// <summary>What a buyer calls the finished drink (vs. the grain that goes in).</summary>
        public static string DrinkName(SpiritStyle s) => s switch
        {
            SpiritStyle.CornWhiskey  => "corn whiskey",
            SpiritStyle.Rye          => "rye whiskey",
            SpiritStyle.MaltedBarley => "malt whiskey",
            SpiritStyle.Wheat        => "wheat whiskey",
            SpiritStyle.SugarShine   => "sugar shine",
            SpiritStyle.Molasses     => "rum",
            _ => s.ToString()
        };
    }

    /// <summary>
    /// A player-authored batch plan (design doc v3 §3.1): what you intend to
    /// make this run — spirit style, batch size, target proof + tolerance, and
    /// an optional flavour lean. It's intent, not a contract: you can miss your
    /// own target and still sell the result to a back-door buyer.
    /// </summary>
    [System.Serializable]
    public struct BatchPlan
    {
        public SpiritStyle spiritStyle;
        public float quantityGallons;
        public float targetProof;
        public float proofTolerance;
        public FlavorRequest flavorRequest;

        // Rough "ambition" of the plan — scales tolerance tightness + quantity.
        // Placeholder until the batch-planning UI lands (project-plan M2).
        public int difficulty;
    }

    /// <summary>
    /// Rolls a starter <see cref="BatchPlan"/>. Prototype stand-in for the
    /// batch-planning UI (M2). Not to be confused with the future
    /// <c>BuyerGenerator</c>, which will produce buyer preference profiles.
    /// </summary>
    public static class BatchPlanGenerator
    {
        public static BatchPlan GeneratePlan(int difficulty = 1)
        {
            var plan = new BatchPlan
            {
                spiritStyle = (SpiritStyle)Random.Range(0, 6),
                targetProof = 80f + Random.value * 20f, // 80-100 proof range
                proofTolerance = Mathf.Max(1f, 6f - difficulty * 1f),
                quantityGallons = 3f + difficulty * 1.5f,
                flavorRequest = FlavorRequest.None,
                difficulty = difficulty
            };

            if (difficulty >= 2)
            {
                plan.flavorRequest = (FlavorRequest)Random.Range(1, 5); // skip None
            }

            return plan;
        }
    }
}
