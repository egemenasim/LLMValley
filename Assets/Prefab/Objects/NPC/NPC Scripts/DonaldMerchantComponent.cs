using LLMValley.Items;
using LLMValley.NPCChat;
using LLMValley.Player;
using UnityEngine;
using Systems.Calendar;

namespace LLMValley.NPCShop
{
    [RequireComponent(typeof(NPCRelationshipStats))]
    public class DonaldMerchantComponent : NPCMerchantComponent
    {
        private NPCRelationshipStats _stats;
        private int _goldSoldAccumulator = 0;

        private int _soldToday = 0;
        private string _lastTransactionDate = ""; 

        private void Awake()
        {
            _stats = GetComponent<NPCRelationshipStats>();
            LoadData();
        }

        public int DailyLimit
        {
            get
            {
                int trust = _stats != null ? _stats.Trust : 0;
                return 200 + (trust / 20) * 100;
            }
        }

        public int RemainingLimit
        {
            get
            {
                CheckResetDailyLimit();
                return Mathf.Max(0, DailyLimit - _soldToday);
            }
        }

        private void CheckResetDailyLimit()
        {
            if (CalendarSystem.Instance != null)
            {
                var currentDate = CalendarSystem.Instance.GetCurrentDate();
                string dateString = $"{currentDate.year}-{currentDate.season}-{currentDate.day}";
                
                if (_lastTransactionDate != dateString)
                {
                    _soldToday = 0;
                    _lastTransactionDate = dateString;
                    SaveData();
                }
            }
        }

        public override bool TrySell(ItemStack stack, PlayerInventory inventory, PlayerWallet wallet, out string resultMessage)
        {
            CheckResetDailyLimit();

            if (stack == null || stack.item == null || stack.quantity <= 0)
            {
                resultMessage = "Invalid item.";
                return false;
            }

            int sellPrice = GetSellPrice(stack.item);
            
            if (_soldToday + sellPrice > DailyLimit)
            {
                resultMessage = $"Donald has a daily limit of {DailyLimit}g. You can only sell {RemainingLimit}g more today.";
                return false;
            }

            bool success = base.TrySell(stack, inventory, wallet, out resultMessage);

            if (success)
            {
                _soldToday += sellPrice;
                _goldSoldAccumulator += sellPrice;
                
                if (_goldSoldAccumulator >= 50)
                {
                    int trustGain = _goldSoldAccumulator / 50;
                    _goldSoldAccumulator %= 50;
                    
                    if (_stats != null)
                    {
                        _stats.AddTrust(trustGain);
                        Debug.Log($"[DonaldMerchant] Sold items. Added {trustGain} Trust. Current Trust: {_stats.Trust}");
                    }
                }
                SaveData();
                
                resultMessage += $" (Daily Limit: {RemainingLimit}g)";
            }

            return success;
        }

        private void SaveData()
        {
            PlayerPrefs.SetInt("Donald_SoldToday", _soldToday);
            PlayerPrefs.SetString("Donald_LastDate", _lastTransactionDate);
            PlayerPrefs.SetInt("Donald_SoldAccumulator", _goldSoldAccumulator);
            PlayerPrefs.Save();
        }

        private void LoadData()
        {
            _soldToday = PlayerPrefs.GetInt("Donald_SoldToday", 0);
            _lastTransactionDate = PlayerPrefs.GetString("Donald_LastDate", "");
            _goldSoldAccumulator = PlayerPrefs.GetInt("Donald_SoldAccumulator", 0);
        }
    }
}
