using UnityEngine;
using LLMValley.Items;

namespace LLMValley.Player
{
    public class SeedToolAction : MonoBehaviour, IToolAction
    {
        public ItemType ItemType => ItemType.Seed;

        public bool CanUse() => true;

        public void Use()
        {
        }
    }
}
