using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace LLMValley.NPCChat
{
    public class NPCChatUIManager : MonoBehaviour, IDialog
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private Image portraitImage;
        [SerializeField] private ScrollRect historyScrollRect;
        [SerializeField] private Transform messageContainer;
        [SerializeField] private TMP_Text userMessageTemplate;
        [SerializeField] private TMP_Text assistantMessageTemplate;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject loadingIndicator;

        private readonly List<GameObject> activeMessageObjects = new();
        private NPCChatAgent currentAgent;
        private bool isSending;

        public static NPCChatUIManager Instance { get; private set; }
        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;
        public NPCChatAgent CurrentAgent => currentAgent;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            if (sendButton != null)
            {
                sendButton.onClick.AddListener(SubmitInput);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseConversation);
            }
        }

        // IDialog — CloseDialog is called by DialogInputManager when Escape is pressed.
        public void CloseDialog() => CloseConversation();

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            // Enter to send (Escape is handled globally by DialogInputManager).
            if (Keyboard.current != null &&
                Keyboard.current.enterKey.wasPressedThisFrame &&
                !Keyboard.current.leftShiftKey.isPressed &&
                !Keyboard.current.rightShiftKey.isPressed)
            {
                SubmitInput();
            }
        }

        public static NPCChatUIManager FindExisting()
        {
            return Instance != null ? Instance : FindFirstObjectByType<NPCChatUIManager>(FindObjectsInactive.Include);
        }

        public void OpenConversation(NPCChatAgent agent)
        {
            if (agent == null || panelRoot == null)
            {
                return;
            }

            currentAgent = agent;
            ClearMessages();
            panelRoot.SetActive(true);

            if (titleLabel != null)
            {
                titleLabel.text = agent.PersonaDisplayName;
            }

            if (portraitImage != null)
            {
                portraitImage.sprite = agent.PersonaPortrait;
                portraitImage.enabled = agent.PersonaPortrait != null;
            }

            // Lock player movement globally while the chat is open.
            PlayerInputLock.Lock();
            DialogInputManager.Register(this);

            RenderHistory(agent.CurrentConversation.messages);
            SetLoading(false, agent.ChatAvailabilityMessage);

            if (inputField != null)
            {
                inputField.text = string.Empty;
                if (agent.CanSendMessages)
                {
                    EventSystem.current?.SetSelectedGameObject(inputField.gameObject);
                    inputField.ActivateInputField();
                }
            }
        }

        public void RenderHistory(IReadOnlyList<NPCChatMessage> messages)
        {
            ClearMessages();
            if (messages == null)
            {
                return;
            }

            foreach (var message in messages)
            {
                AppendMessage(message);
            }

            ScrollToBottom();
        }

        public void AppendMessage(NPCChatMessage message)
        {
            if (messageContainer == null || currentAgent == null || message == null)
            {
                return;
            }

            var template = ResolveTemplate(message);
            if (template == null)
            {
                Debug.LogWarning($"[NPC Chat] Missing message template for role '{message.role}'.", this);
                return;
            }

            var messageInstance = Instantiate(template, messageContainer, false);
            messageInstance.gameObject.SetActive(true);
            messageInstance.text = message.content;
            ConfigureMessageInstance(messageInstance, string.Equals(message.role, "user"));
            activeMessageObjects.Add(messageInstance.gameObject);
            ScrollToBottom();
        }

        public void SetLoading(bool loading, string status = null)
        {
            isSending = loading;
            var canSendMessages = currentAgent == null || currentAgent.CanSendMessages;

            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(loading);
            }

            if (sendButton != null)
            {
                sendButton.interactable = !loading && canSendMessages;
            }

            if (inputField != null)
            {
                inputField.interactable = !loading && canSendMessages;
            }

            if (statusLabel != null)
            {
                statusLabel.text = string.IsNullOrWhiteSpace(status)
                    ? (loading ? "NPC is thinking..." : string.Empty)
                    : status;
            }
        }

        public void ShowError(string message)
        {
            SetLoading(false, message);
        }

        public void CloseConversation()
        {
            if (!IsOpen)
            {
                return;
            }

            var agent = currentAgent;
            currentAgent = null;
            SetLoading(false, string.Empty);
            panelRoot.SetActive(false);
            ClearMessages();
            DialogInputManager.Unregister(this);
            PlayerInputLock.Unlock();
            agent?.EndConversation();
        }

        private void SubmitInput()
        {
            if (currentAgent == null || inputField == null || isSending)
            {
                return;
            }

            var message = inputField.text.Trim();
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            inputField.text = string.Empty;
            currentAgent.SendPlayerMessage(message);
            EventSystem.current?.SetSelectedGameObject(inputField.gameObject);
            inputField.ActivateInputField();
        }

        private void ClearMessages()
        {
            foreach (var messageObject in activeMessageObjects)
            {
                if (messageObject != null)
                {
                    Destroy(messageObject);
                }
            }

            activeMessageObjects.Clear();
        }

        private void ScrollToBottom()
        {
            Canvas.ForceUpdateCanvases();
            if (messageContainer is RectTransform rectTransform)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
            }
            if (historyScrollRect != null)
            {
                historyScrollRect.verticalNormalizedPosition = 0f;
            }
        }

        private TMP_Text ResolveTemplate(NPCChatMessage message)
        {
            var isUserMessage = string.Equals(message.role, "user");
            if (isUserMessage && userMessageTemplate != null)
            {
                return userMessageTemplate;
            }

            if (!isUserMessage && assistantMessageTemplate != null)
            {
                return assistantMessageTemplate;
            }

            return userMessageTemplate != null ? userMessageTemplate : assistantMessageTemplate;
        }

        private static void ConfigureMessageInstance(TMP_Text messageInstance, bool isUserMessage)
        {
            var rectTransform = messageInstance.rectTransform;
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            messageInstance.textWrappingMode = TextWrappingModes.Normal;
            messageInstance.overflowMode = TextOverflowModes.Overflow;
            messageInstance.alignment = isUserMessage
                ? TextAlignmentOptions.MidlineRight
                : TextAlignmentOptions.MidlineLeft;

            var preferredHeight = Mathf.Max(36f, messageInstance.GetPreferredValues(messageInstance.text, rectTransform.rect.width, 0f).y);

            var layoutElement = messageInstance.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = messageInstance.gameObject.AddComponent<LayoutElement>();
            }

            layoutElement.minHeight = preferredHeight;
            layoutElement.preferredHeight = preferredHeight + 12f;
            layoutElement.flexibleWidth = 1f;

            rectTransform.sizeDelta = new Vector2(0f, layoutElement.preferredHeight);
            messageInstance.transform.SetAsLastSibling();
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);
        }
    }
}
