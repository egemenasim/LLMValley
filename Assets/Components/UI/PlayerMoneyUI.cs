using UnityEngine;
using TMPro;

namespace LLMValley.Player
{
    public class PlayerMoneyUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI moneyText;

        private PlayerWallet boundWallet;

        private void OnEnable()
        {
            PlayerWallet.OnInstanceChanged += BindWallet;
            BindWallet(PlayerWallet.Instance);
        }

        private void OnDisable()
        {
            PlayerWallet.OnInstanceChanged -= BindWallet;
            UnbindWallet();
        }

        private void BindWallet(PlayerWallet wallet)
        {
            if (boundWallet == wallet)
            {
                return;
            }

            UnbindWallet();

            boundWallet = wallet;
            if (boundWallet != null)
            {
                boundWallet.OnGoldChanged.AddListener(UpdateMoneyUI);
                UpdateMoneyUI(boundWallet.CurrentGold);
            }
        }

        private void UnbindWallet()
        {
            if (boundWallet != null)
            {
                boundWallet.OnGoldChanged.RemoveListener(UpdateMoneyUI);
                boundWallet = null;
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
