using UnityEngine;

namespace MoonshineSim.Core
{
    public struct Appraisal
    {
        public int price;
        public int rapportGain;
        public float matchScore;   // 0..1, how well the jar fit the buyer
    }

    /// <summary>
    /// Prices a finished jar against a buyer's preference profile
    /// (design doc v3 §6.1). No hard fail — a poor match just pays little.
    /// </summary>
    public static class BatchAppraisal
    {
        public static Appraisal Appraise(BatchState batch, Buyer buyer)
        {
            float styleMatch = batch.style == buyer.preferredStyle ? 1f : 0.4f;

            float proofMatch;
            if (batch.currentProof >= buyer.proofMin && batch.currentProof <= buyer.proofMax)
            {
                proofMatch = 1f;
            }
            else
            {
                float dist = batch.currentProof < buyer.proofMin
                    ? buyer.proofMin - batch.currentProof
                    : batch.currentProof - buyer.proofMax;
                proofMatch = Mathf.Clamp01(1f - dist / 20f);
            }

            // batch.smoothness 0..1 -> lean -1..1, compare to the buyer's wish.
            float batchLean = batch.smoothness * 2f - 1f;
            float leanMatch = Mathf.Clamp01(1f - Mathf.Abs(buyer.smoothLean - batchLean) * 0.5f);

            float match = styleMatch * 0.4f + proofMatch * 0.4f + leanMatch * 0.2f;

            float qualityFactor = 0.5f + batch.heartsQuality * 0.5f;
            float rapportMult = 1f + buyer.rapport / 200f;      // up to +50% at rapport 100
            const float basePerLitre = 45f;

            int price = Mathf.RoundToInt(
                basePerLitre * Mathf.Max(0.25f, batch.heartsVolumeL) *
                match * qualityFactor * buyer.priceSensitivity * rapportMult);

            int rapportGain = Mathf.RoundToInt(Mathf.Lerp(-3f, 9f, match));

            return new Appraisal { price = price, rapportGain = rapportGain, matchScore = match };
        }
    }
}
