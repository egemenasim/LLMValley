using System;
using System.Collections.Generic;
using LLMValley.Items;
using LLMValley.NPCChat;
using LLMValley.Player;
using UnityEngine;

namespace LLMValley.NPCShop
{
    [Serializable]
    public class NPCShopListing
    {
        public ItemData item;

        public int Price
        {
            get
            {
                return item == null ? 0 : Mathf.Max(0, Mathf.RoundToInt(item.baseValue));
            }
        }
    }

    /// <summary>
    /// Attach to NPCs that should sell items to the player.
    /// Uses the project's existing ItemData ScriptableObject format.
    /// </summary>
    public class NPCSellComponent : MonoBehaviour
    {
        [Header("Shop")]
        [SerializeField] private string shopDisplayName = "NPC Shop";
        [SerializeField] private bool openShopOnInteract = true;
        [SerializeField] private List<NPCShopListing> stock = new();

        public string ShopDisplayName => string.IsNullOrWhiteSpace(shopDisplayName) ? gameObject.name : shopDisplayName;
        public bool OpenShopOnInteract => openShopOnInteract;
        public IReadOnlyList<NPCShopListing> Stock => stock;

        public int GetPurchasePrice(NPCShopListing listing)
        {
            if (listing == null || listing.item == null)
            {
                return 0;
            }

            var basePrice = Mathf.Max(0, listing.item.baseValue);
            var friendshipDiscount = GetFriendshipPriceModifier();
            return Mathf.Max(0, Mathf.RoundToInt(basePrice * (1f - friendshipDiscount)));
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

        public bool TryPurchase(
            NPCShopListing listing,
            PlayerInventory inventory,
            PlayerWallet wallet,
            out string resultMessage)
        {
            resultMessage = string.Empty;

            if (listing == null || listing.item == null)
            {
                resultMessage = "This item is not configured.";
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

            if (!inventory.CanCollect(listing.item, 1))
            {
                resultMessage = "Inventory is full.";
                return false;
            }

            var price = GetPurchasePrice(listing);
            if (!wallet.TrySpend(price))
            {
                resultMessage = "Not enough gold.";
                return false;
            }

            if (!inventory.TryCollectItem(listing.item, 1))
            {
                wallet.AddGold(price);
                resultMessage = "Inventory is full.";
                return false;
            }

            resultMessage = $"Bought 1x {listing.item.itemName} for {price} gold.";
            return true;
        }
    }
}
