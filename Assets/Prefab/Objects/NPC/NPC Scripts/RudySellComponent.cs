using System.Collections.Generic;
using LLMValley.NPCChat;
using LLMValley.Player;
using UnityEngine;

namespace LLMValley.NPCShop
{
    [RequireComponent(typeof(NPCRelationshipStats))]
    public class RudySellComponent : NPCSellComponent
    {
        [Header("Rudy Config")]
        public NPCShopListing goldenFishingRod;
        public NPCShopListing goldenWateringCan;
        public NPCShopListing goldenHoe;

        private NPCRelationshipStats _stats;

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
                
                List<NPCShopListing> currentStock = new List<NPCShopListing>(baseStock);
                
                if (trust >= 30 && goldenFishingRod != null && goldenFishingRod.item != null)
                {
                    currentStock.Add(goldenFishingRod);
                }
                if (trust >= 60 && goldenWateringCan != null && goldenWateringCan.item != null)
                {
                    currentStock.Add(goldenWateringCan);
                }
                if (trust >= 90 && goldenHoe != null && goldenHoe.item != null)
                {
                    currentStock.Add(goldenHoe);
                }
                
                return currentStock;
            }
        }

        public override bool TryPurchase(NPCShopListing listing, PlayerInventory inventory, PlayerWallet wallet, out string resultMessage)
        {
            bool success = base.TryPurchase(listing, inventory, wallet, out resultMessage);

            if (success)
            {
                if (_stats != null)
                {
                    _stats.AddTrust(5);
                    Debug.Log($"[RudySellComponent] Bought item. Added 5 Trust. Current Trust: {_stats.Trust}");
                }
            }

            return success;
        }
    }
}
