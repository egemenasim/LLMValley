using LLMValley.NPCChat;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LLMValley.Menu
{
    public class MainMenuController : MonoBehaviour
    {
        private const string GameSceneName = "SCENE_Sadi_FarmTest";

        [SerializeField] private GameObject creditsPanel;
        [SerializeField] private GameObject apiKeyPanel;
        [SerializeField] private TMP_InputField apiKeyInput;
        [SerializeField] private TMP_Text apiKeyStatusLabel;

        [Header("Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button apiKeyButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button saveApiKeyButton;
        [SerializeField] private Button closeCreditsButton;
        [SerializeField] private Button closeApiKeyButton;

        private void Awake()
        {
            CacheReferences();
            BindButtons();
            CloseCredits();
            CloseApiKey();

            if (apiKeyInput != null)
            {
                apiKeyInput.text = OpenRouterApiKeyStore.ApiKey;
            }
        }

        private void OnDestroy()
        {
            UnbindButtons();
        }

        public void Play()
        {
            SceneManager.LoadScene(GameSceneName);
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void OpenCredits()
        {
            if (creditsPanel != null)
            {
                creditsPanel.SetActive(true);
            }
        }

        public void CloseCredits()
        {
            if (creditsPanel != null)
            {
                creditsPanel.SetActive(false);
            }
        }

        public void OpenApiKey()
        {
            if (apiKeyPanel != null)
            {
                apiKeyPanel.SetActive(true);
            }

            if (apiKeyInput != null)
            {
                apiKeyInput.text = OpenRouterApiKeyStore.ApiKey;
                apiKeyInput.ActivateInputField();
            }

            SetApiKeyStatus(string.Empty);
        }

        public void CloseApiKey()
        {
            if (apiKeyPanel != null)
            {
                apiKeyPanel.SetActive(false);
            }
        }

        public void SaveApiKey()
        {
            var key = apiKeyInput != null ? apiKeyInput.text : string.Empty;
            OpenRouterApiKeyStore.Save(key);
            SetApiKeyStatus(string.IsNullOrWhiteSpace(key) ? "API key cleared." : "API key saved.");
        }

        private void SetApiKeyStatus(string message)
        {
            if (apiKeyStatusLabel != null)
            {
                apiKeyStatusLabel.text = message;
            }
        }

        private void CacheReferences()
        {
            var transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var item in transforms)
            {
                if (item == null)
                {
                    continue;
                }

                switch (item.name)
                {
                    case "Credits Panel":
                        creditsPanel ??= item.gameObject;
                        break;
                    case "API Key Panel":
                        apiKeyPanel ??= item.gameObject;
                        break;
                    case "API Key Input":
                        apiKeyInput ??= item.GetComponent<TMP_InputField>();
                        break;
                    case "Play Button":
                        playButton ??= item.GetComponent<Button>();
                        break;
                    case "API Key Button":
                        apiKeyButton ??= item.GetComponent<Button>();
                        break;
                    case "Credits Button":
                        creditsButton ??= item.GetComponent<Button>();
                        break;
                    case "Quit Button":
                        quitButton ??= item.GetComponent<Button>();
                        break;
                    case "Save Button":
                        saveApiKeyButton ??= item.GetComponent<Button>();
                        break;
                }
            }

            foreach (var item in transforms)
            {
                if (item != null && item.name == "Close Button")
                {
                    CacheCloseButton(item);
                }
            }

            if (apiKeyStatusLabel == null && apiKeyPanel != null)
            {
                var status = apiKeyPanel.transform.Find("Status");
                apiKeyStatusLabel = status != null ? status.GetComponent<TMP_Text>() : null;
            }
        }

        private void CacheCloseButton(Transform closeTransform)
        {
            var button = closeTransform.GetComponent<Button>();
            if (button == null)
            {
                return;
            }

            if (creditsPanel != null && closeTransform.IsChildOf(creditsPanel.transform))
            {
                closeCreditsButton ??= button;
            }
            else if (apiKeyPanel != null && closeTransform.IsChildOf(apiKeyPanel.transform))
            {
                closeApiKeyButton ??= button;
            }
        }

        private void BindButtons()
        {
            UnbindButtons();

            if (playButton != null) playButton.onClick.AddListener(Play);
            if (apiKeyButton != null) apiKeyButton.onClick.AddListener(OpenApiKey);
            if (creditsButton != null) creditsButton.onClick.AddListener(OpenCredits);
            if (quitButton != null) quitButton.onClick.AddListener(Quit);
            if (saveApiKeyButton != null) saveApiKeyButton.onClick.AddListener(SaveApiKey);
            if (closeCreditsButton != null) closeCreditsButton.onClick.AddListener(CloseCredits);
            if (closeApiKeyButton != null) closeApiKeyButton.onClick.AddListener(CloseApiKey);
        }

        private void UnbindButtons()
        {
            if (playButton != null) playButton.onClick.RemoveListener(Play);
            if (apiKeyButton != null) apiKeyButton.onClick.RemoveListener(OpenApiKey);
            if (creditsButton != null) creditsButton.onClick.RemoveListener(OpenCredits);
            if (quitButton != null) quitButton.onClick.RemoveListener(Quit);
            if (saveApiKeyButton != null) saveApiKeyButton.onClick.RemoveListener(SaveApiKey);
            if (closeCreditsButton != null) closeCreditsButton.onClick.RemoveListener(CloseCredits);
            if (closeApiKeyButton != null) closeApiKeyButton.onClick.RemoveListener(CloseApiKey);
        }
    }
}
