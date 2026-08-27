namespace MoonshineSim.Core
{
    public sealed class Upgrade
    {
        public string name;
        public string blurb;
        public int cost;
    }

    /// <summary>
    /// The linear come-up: spend cash at the workbench to work up to a licence
    /// and a real facility (design doc v3 §5, §6.1). A branching shop can come
    /// later — this is the MVP spine.
    /// </summary>
    public static class UpgradeTrack
    {
        public static readonly Upgrade[] Steps =
        {
            new Upgrade { name = "Bigger boiler",       cost = 150,  blurb = "double the batch size" },
            new Upgrade { name = "Copper worm",         cost = 350,  blurb = "steadier gauges, tighter cuts" },
            new Upgrade { name = "Proper pot still",    cost = 750,  blurb = "no pressure, cleaner spirit" },
            new Upgrade { name = "Mountain hollow",     cost = 1500, blurb = "spring water, room to work" },
            new Upgrade { name = "Reflux column",       cost = 3200, blurb = "near-neutral on demand" },
            new Upgrade { name = "Distiller's licence", cost = 7000, blurb = "go legit — the whole point" },
        };
    }
}
