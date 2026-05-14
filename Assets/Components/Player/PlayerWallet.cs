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

        [Header("Events")]
        public UnityEngine.Events.UnityEvent<int> OnGoldChanged = new UnityEngine.Events.UnityEvent<int>();

        public static PlayerWallet Instance { get; private set; }
        public static event System.Action<PlayerWallet> OnInstanceChanged;

        public void SetGold(int amount)
        {
            currentGold = Mathf.Max(0, amount);
            OnGoldChanged?.Invoke(currentGold);
            SaveWalletChange();
        }

        private void Awake()
        {
            if (currentGold <= 0)
            {
                currentGold = startingGold;
            }

            Instance = this;
            OnInstanceChanged?.Invoke(this);
        }

        private void Start()
        {
            OnGoldChanged?.Invoke(currentGold);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                OnInstanceChanged?.Invoke(null);
            }
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentGold += amount;
            OnGoldChanged?.Invoke(currentGold);
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
            OnGoldChanged?.Invoke(currentGold);
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
