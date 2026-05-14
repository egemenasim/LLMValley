using System.Collections.Generic;
using LLMValley.NPCChat;
using LLMValley.Player;
using UnityEngine;

namespace LLMValley.NPCShop
{
    [RequireComponent(typeof(NPCRelationshipStats))]
    public class HelenSellComponent : NPCSellComponent
    {
        [Header("Helen Config")]
        [Tooltip("The pool of all possible seeds Helen can sell, beyond the base items configured in the base component. For every 30 Trust, 2 items from this pool are added to the shop.")]
        public List<NPCShopListing> extraSeedsPool = new();

        private NPCRelationshipStats _stats;
        private int _goldSpentAccumulator = 0;

        private void Awake()
        {
            _stats = GetComponent<NPCRelationshipStats>();
        }

        public override IReadOnlyList<NPCShopListing> Stock
        {
            get
            {
                var baseStock = base.Stock;
                int trust = _stats != null ? _stats.Trust : 0;
                
                // For each 30 points we gain in Trust, 2 more seed options will be added.
                int extraItemsCount = (trust / 30) * 2;

                if (extraItemsCount <= 0 || extraSeedsPool == null || extraSeedsPool.Count == 0)
                {
                    return baseStock;
                }

                List<NPCShopListing> currentStock = new List<NPCShopListing>(baseStock);
                for (int i = 0; i < extraItemsCount && i < extraSeedsPool.Count; i++)
                {
                    currentStock.Add(extraSeedsPool[i]);
                }
                return currentStock;
            }
        }

        public override bool TryPurchase(NPCShopListing listing, PlayerInventory inventory, PlayerWallet wallet, out string resultMessage)
        {
            int price = GetPurchasePrice(listing);
            bool success = base.TryPurchase(listing, inventory, wallet, out resultMessage);

            if (success)
            {
                _goldSpentAccumulator += price;
                if (_goldSpentAccumulator >= 50)
                {
                    int trustGain = _goldSpentAccumulator / 50;
                    _goldSpentAccumulator %= 50;
                    
                    if (_stats != null)
                    {
                        _stats.AddTrust(trustGain);
                        Debug.Log($"[HelenSellComponent] Spent {price}g. Total accumulator reached threshold. Added {trustGain} Trust. Current Trust: {_stats.Trust}");
                    }
                }
            }

            return success;
        }
    }
}
