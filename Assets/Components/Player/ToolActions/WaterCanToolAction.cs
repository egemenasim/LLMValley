using UnityEngine;
using LLMValley.Items;

namespace LLMValley.Player
{
    public class WaterCanToolAction : MonoBehaviour, IToolAction
    {
        public ItemType ItemType => ItemType.WaterCan;

        public bool CanUse() => true;

        public void Use()
        {
        }
    }
}
