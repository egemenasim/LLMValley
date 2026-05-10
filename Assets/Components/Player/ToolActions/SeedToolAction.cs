using UnityEngine;
using LLMValley.Items;

namespace LLMValley.Player
{
    public class SeedToolAction : MonoBehaviour, IToolAction
    {
        public ItemType ItemType => ItemType.Seed;

        public bool CanUse()
        {
            var controller = GetComponentInParent<PlayerToolController>();
            if (controller == null)
                return false;

            if (!controller.TryGetTargetFarmableArea(out var area) || area == null)
                return false;

            if (!area.IsPlayerInRange())
                return false;

            if (area.HasPlant)
                return false;

            // Requires the soil to be tilled first.
            if (!area.IsTilled)
            {
                Debug.LogWarning($"[SeedToolAction] CanUse blocked: area '{area.name}' is not tilled.");
                return false;
            }

            var stack = controller.GetSelectedStack();
            if (stack == null || !stack.IsValid || stack.quantity <= 0)
                return false;

            // Must be a seed item.
            return stack.item.itemType == ItemType.Seed;
        }

        public void Use()
        {
            var controller = GetComponentInParent<PlayerToolController>();
            if (controller == null)
                return;

            if (!controller.TryGetTargetFarmableArea(out var area) || area == null)
            {
                Debug.LogWarning("[SeedToolAction] No FarmableArea targeted under mouse.");
                return;
            }

            Debug.Log($"[SeedToolAction] Use() start. Area: {area.name}, HasPlant: {area.HasPlant}, IsTilled: {area.IsTilled}");

            var stack = controller.GetSelectedStack();
            if (stack == null || !stack.IsValid)
            {
                Debug.LogWarning("[SeedToolAction] No valid seed stack selected.");
                return;
            }

            Debug.Log($"[SeedToolAction] Try plant seed: {stack.item.itemName} (id: {stack.item.itemID}) on area: {area.name}");

            Plantable plantPrefab = controller.FindPlantPrefabForSeed(stack.item);
            bool plantSuccess = false;

            if (plantPrefab != null)
            {
                plantSuccess = area.Plant(plantPrefab);
            }
            else
            {
                // Fallback to data-driven planting if no prefab is found.
                plantSuccess = area.Plant(stack.item);
            }

            if (!plantSuccess)
            {
                Debug.LogWarning($"[SeedToolAction] Plant() failed on area: {area.name}. HasPlant: {area.HasPlant}, IsTilled: {area.IsTilled}");
                return;
            }

            Debug.Log($"[SeedToolAction] After Plant(): area.HasPlant={area.HasPlant}, currentPlant={(area.CurrentPlant != null ? area.CurrentPlant.name : "null")}");

            Debug.Log($"[SeedToolAction] Plant() success on area: {area.name}. Consuming 1 seed from inventory.");

            // Consume 1 seed from the selected stack.
            controller.ConsumeSelectedStackItem(1);
        }
    }
}
