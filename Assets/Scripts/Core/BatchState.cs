namespace MoonshineSim.Core
{
    /// <summary>Linear progress of one batch through the workshop stations.</summary>
    public enum BatchStage
    {
        None,        // nothing started
        GrainChosen, // picked a grain at a bin — ready to mash
        Mashing,     // mash tub working
        Mashed,      // ready to ferment
        Fermenting,  // fermenter working
        WashReady,   // wash done — ready to distill
        Distilling,  // still run in progress
        Distilled,   // hearts in the jar — needs proofing
        Proofed,     // watered down — ready to sell
        Sold
    }

    /// <summary>
    /// Plain-C# state for one batch. A <c>BatchController</c> MonoBehaviour owns
    /// one and drives it from world-station interactions (no Unity dependency
    /// here — the ProofingUtility pattern).
    /// </summary>
    public class BatchState
    {
        public BatchStage stage = BatchStage.None;
        public SpiritStyle style;

        public float batchSizeGallons = 3f; // set by how much grain you pour into the mash tub (rig-capped)
        public float washAbv;        // % ABV after fermentation
        public float heartsVolumeL;  // litres of hearts kept from the still run
        public float heartsQuality;  // 0..1 — cut precision from the still run
        public float smoothness;     // 0..1 — how clean/light (vs characterful) it is
        public float currentProof;   // present strength of the jar, in proof

        public bool HasJar => stage >= BatchStage.Distilled && stage < BatchStage.Sold;
        public bool ReadyToDistill => stage == BatchStage.WashReady;
    }
}
