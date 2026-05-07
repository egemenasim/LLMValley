using UnityEngine;
using LLMValley.Items;

namespace LLMValley.Player
{
    public class RodToolAction : MonoBehaviour, IToolAction
    {
        public ItemType ItemType => ItemType.Rod;

        public bool CanUse() => true;

        public void Use()
        {
        }
    }
}
