using UnityEngine;
using LLMValley.Items;

namespace LLMValley.Player
{
    public class WaterCanToolAction : MonoBehaviour, IToolAction
    {
        public ItemType ItemType => ItemType.WaterCan;

        public bool CanUse()
        {
            var controller = GetComponentInParent<PlayerToolController>();
            if (controller == null)
                return false;

            if (!controller.TryGetTargetFarmableArea(out var area) || area == null)
                return false;

            if (!area.IsPlayerInRange())
                return false;

            return area.HasPlant;
        }

        public void Use()
        {
            var controller = GetComponentInParent<PlayerToolController>();
            if (controller == null)
                return;

            if (!controller.TryGetTargetFarmableArea(out var area) || area == null)
                return;

            // Current growth system advances on day change if watered today.
            area.Water();
        }
    }
}
