using UnityEngine;
using LLMValley.Items;

namespace LLMValley.Player
{
    public class HoeToolAction : MonoBehaviour, IToolAction
    {
        public ItemType ItemType => ItemType.Hoe;

        public bool CanUse()
        {
            var controller = GetComponentInParent<PlayerToolController>();
            if (controller == null)
                return false;

            return controller.TryGetTargetFarmableArea(out _);
        }

        public void Use()
        {
            var controller = GetComponentInParent<PlayerToolController>();
            if (controller == null)
                return;

            if (!controller.TryGetTargetFarmableArea(out var area) || area == null)
                return;

            area.Till();
        }
    }
}
