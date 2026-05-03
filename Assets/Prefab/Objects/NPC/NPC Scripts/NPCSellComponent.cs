using System;
using System.Collections.Generic;
using LLMValley.Items;
using LLMValley.Player;
using UnityEngine;

namespace LLMValley.NPCShop
{
    [Serializable]
    public class NPCShopListing
    {
        public ItemData item;
        [Min(1)] public int quantity = 1;
        [Min(-1)] public int priceOverride = -1;

        public int Price
        {
            get
            {
                if (priceOverride >= 0)
                {
                    return priceOverride;
                }

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

            if (!inventory.CanCollect(listing.item, listing.quantity))
            {
                resultMessage = "Inventory is full.";
                return false;
            }

            var price = Mathf.Max(0, listing.Price);
            if (!wallet.TrySpend(price))
            {
                resultMessage = "Not enough gold.";
                return false;
            }

            inventory.TryCollectItem(listing.item, listing.quantity);
            resultMessage = $"Bought {listing.quantity}x {listing.item.itemName} for {price} gold.";
            return true;
        }
    }
}
