using UnityEngine;

namespace LLMValley.Items
{
    [CreateAssetMenu(fileName = "New Key", menuName = "LLMValley/Items/KeyItem", order = 1)]
    public class KeyItemData : ItemData
    {
        [Header("Key")]
        [Tooltip("Unique identifier used for matching doors/gates.")]
        public string keyId;

        [Tooltip("Display name for the key.")]
        public string keyName;

        [Tooltip("Optional area identifier that this key unlocks.")]
        public string areaId;
    }
}
