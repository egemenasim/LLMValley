using UnityEngine;

namespace LLMValley.SaveSystem
{
    public enum FarmableAreaState
    {
        Locked,        // Not yet tilled
        Empty,         // Tilled but no plant
        Planted,       // Plant placed, not yet growing
        Growing,       // Plant is growing
        ReadyToHarvest,
        Harvested
    }
}
