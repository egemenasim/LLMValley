using UnityEngine;
using LLMValley.Items;

namespace LLMValley.Player
{
    public class HoeToolAction : MonoBehaviour, IToolAction
    {
        public ItemType ItemType => ItemType.Hoe;

        public bool CanUse() => true;

        public void Use()
        {
        }
    }
}
