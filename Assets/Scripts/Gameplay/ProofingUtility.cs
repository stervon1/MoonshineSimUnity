using UnityEngine;

namespace MoonshineSim.Gameplay
{
    /// <summary>
    /// Proofing mini-task math: cut hearts down to bottling proof by
    /// adding water (design doc section 4.4). Kept as a simple "dial it
    /// in" tactile task rather than a puzzle with a hard-fail state.
    /// </summary>
    public static class ProofingUtility
    {
        /// <summary>
        /// Pearson's Square with water (0 proof):
        /// waterVolume = heartsVolume * (heartsProof - targetProof) / targetProof
        /// </summary>
        public static float CalculateWaterToAdd(float heartsVolume, float heartsProof, float targetProof)
        {
            if (targetProof <= 0f) return 0f;
            return heartsVolume * (heartsProof - targetProof) / targetProof;
        }

        public static float ScoreAgainstSpec(float finalProof, float targetProof, float tolerance)
        {
            float distance = Mathf.Abs(finalProof - targetProof);
            return Mathf.Clamp01(1f - (distance / tolerance));
        }
    }
}
