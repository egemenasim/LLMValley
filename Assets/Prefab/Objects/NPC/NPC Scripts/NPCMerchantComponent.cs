using System.Collections.Generic;
using LLMValley.Items;
using LLMValley.NPCChat;
using LLMValley.Player;
using UnityEngine;

namespace LLMValley.NPCShop
{
    /// <summary>
    /// Attach to NPCs that buy items from the player.
    /// Uses the same combined NPC chat/shop UI, but lists sellable inventory items with Sell buttons.
    /// </summary>
    public class NPCMerchantComponent : MonoBehaviour
    {
        [Header("Merchant")]
        [SerializeField] private string merchantDisplayName = "Merchant";
        [SerializeField] private bool openMerchantOnInteract = true;
        [SerializeField] private List<ItemType> acceptedItemTypes = new() { ItemType.Crop, ItemType.Fish, ItemType.Seed, ItemType.Misc };
        [SerializeField, Min(0f)] private float sellPriceMultiplier = 1f;

        public string MerchantDisplayName => string.IsNullOrWhiteSpace(merchantDisplayName) ? gameObject.name : merchantDisplayName;
        public bool OpenMerchantOnInteract => openMerchantOnInteract;

        public bool CanSell(ItemData item)
        {
            return item != null && acceptedItemTypes.Contains(item.itemType);
        }

        public int GetSellPrice(ItemData item)
        {
            if (item == null)
            {
                return 0;
            }

            var friendshipBonus = GetFriendshipPriceModifier();
            return Mathf.Max(0, Mathf.RoundToInt(item.baseValue * sellPriceMultiplier * (1f + friendshipBonus)));
        }

        private float GetFriendshipPriceModifier()
        {
            var relationshipStats = GetComponent<NPCRelationshipStats>();
            if (relationshipStats == null)
            {
                return 0f;
            }

            var friendshipTiers = Mathf.Clamp(relationshipStats.Friendship / 20, 0, 5);
            return friendshipTiers * 0.05f;
        }

        public virtual bool TrySell(ItemStack stack, PlayerInventory inventory, PlayerWallet wallet, out string resultMessage)
        {
            resultMessage = string.Empty;

            if (stack == null || stack.item == null || stack.quantity <= 0)
            {
                resultMessage = "This inventory item is invalid.";
                return false;
            }

            if (inventory == null)
            {
                resultMessage = "Player inventory could not be found.";
                return false;
            }

            if (wallet == null)
            {
                resultMessage = "Player wallet could not be found.";
                return false;
            }

            if (!CanSell(stack.item))
            {
                resultMessage = $"{stack.item.itemName} cannot be sold to this merchant.";
                return false;
            }

            if (!inventory.TryRemoveItem(stack.item, 1))
            {
                resultMessage = $"You do not have any {stack.item.itemName} left.";
                return false;
            }

            var sellPrice = GetSellPrice(stack.item);
            wallet.AddGold(sellPrice);
            resultMessage = $"Sold 1x {stack.item.itemName} for {sellPrice} gold.";
            return true;
        }
    }
}
