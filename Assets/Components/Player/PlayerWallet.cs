using UnityEngine;
using LLMValley.SaveSystem;

namespace LLMValley.Player
{
    /// <summary>
    /// Minimal player wallet used by NPC shops.
    /// Holds the current gold amount and exposes simple spend/add helpers.
    /// </summary>
    public class PlayerWallet : MonoBehaviour
    {
        [Header("Wallet")]
        [SerializeField, Min(0)] private int startingGold = 250;
        [SerializeField, Min(0)] private int currentGold;

        public int CurrentGold => currentGold;

        public void SetGold(int amount)
        {
            currentGold = Mathf.Max(0, amount);
            SaveWalletChange();
        }

        private void Awake()
        {
            if (currentGold <= 0)
            {
                currentGold = startingGold;
            }
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentGold += amount;
            SaveWalletChange();
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0)
            {
                return false;
            }

            if (currentGold < amount)
            {
                return false;
            }

            currentGold -= amount;
            SaveWalletChange();
            return true;
        }

        private void SaveWalletChange()
        {
            if (SaveManager.IsApplyingSaveData)
            {
                return;
            }

            SaveManager.SaveGame();
        }
    }
}
