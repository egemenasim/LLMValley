using System.Collections.Generic;
using LLMValley.NPCChat;
using LLMValley.Player;
using UnityEngine;

namespace LLMValley.NPCShop
{
    [RequireComponent(typeof(NPCRelationshipStats))]
    public class EzekielSellComponent : NPCSellComponent
    {
        [Header("Ezekiel Config")]
        public NPCShopListing key1;
        public NPCShopListing key2;
        public NPCShopListing key3;

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
                
                if (trust >= 10 && key1 != null && key1.item != null)
                {
                    currentStock.Add(key1);
                }
                if (trust >= 40 && key2 != null && key2.item != null)
                {
                    currentStock.Add(key2);
                }
                if (trust >= 70 && key3 != null && key3.item != null)
                {
                    currentStock.Add(key3);
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
                    _stats.AddTrust(20);
                    Debug.Log($"[EzekielSellComponent] Bought key. Added 20 Trust. Current Trust: {_stats.Trust}");
                }
            }

            return success;
        }
    }
}
