using UnityEngine;
using TMPro;

namespace LLMValley.Player
{
    public class PlayerMoneyUI : MonoBehaviour
    {
        [SerializeField] private PlayerWallet playerWallet;
        [SerializeField] private TextMeshProUGUI moneyText;

        private void OnEnable()
        {
            if (playerWallet != null)
            {
                playerWallet.OnGoldChanged.AddListener(UpdateMoneyUI);
                UpdateMoneyUI(playerWallet.CurrentGold);
            }
        }

        private void OnDisable()
        {
            if (playerWallet != null)
            {
                playerWallet.OnGoldChanged.RemoveListener(UpdateMoneyUI);
            }
        }

        private void UpdateMoneyUI(int currentGold)
        {
            if (moneyText != null)
            {
                moneyText.text = currentGold.ToString() + " gold";
            }
        }
    }
}
