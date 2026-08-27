namespace MoonshineSim.Core
{
    /// <summary>
    /// A back-door buyer (design doc v3 §3.2). A standing preference profile —
    /// no explicit orders. You decide which buyer to take a finished jar to;
    /// how close it lands to their profile sets the price.
    /// </summary>
    [System.Serializable]
    public class Buyer
    {
        public string name = "Buyer";
        public SpiritStyle preferredStyle;
        public float proofMin = 90f;
        public float proofMax = 100f;

        /// -1 = wants a characterful, heavier spirit; +1 = wants it smooth/clean.
        public float smoothLean;

        /// 0.7 = tight-fisted, 1.5 = pays well.
        public float priceSensitivity = 1f;

        /// Jars they'll take per visit (grows with rapport later).
        public int appetiteJars = 2;

        /// 0..100.
        public int rapport;

        public float ProofMid => (proofMin + proofMax) * 0.5f;
    }
}
