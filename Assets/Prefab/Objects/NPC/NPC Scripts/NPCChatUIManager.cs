using System.Collections.Generic;
using LLMValley.NPCShop;
using LLMValley.Player;
using LLMValley.Items;
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
        [Header("Relationship Bars")]
        [SerializeField] private GameObject relationshipBarsRoot;
        [SerializeField] private Image loveBarFill;
        [SerializeField] private Image friendshipBarFill;
        [SerializeField] private Image trustBarFill;
        [SerializeField] private ScrollRect historyScrollRect;
        [SerializeField] private Transform messageContainer;
        [SerializeField] private TMP_Text userMessageTemplate;
        [SerializeField] private TMP_Text assistantMessageTemplate;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button sendButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject loadingIndicator;
        [Header("Shop UI")]
        [SerializeField] private GameObject sellPanelRoot;
        [SerializeField] private TMP_Text shopTitleLabel;
        [SerializeField] private TMP_Text shopGoldLabel;
        [SerializeField] private TMP_Text shopStatusLabel;
        [SerializeField] private Transform shopItemsContainer;
        [SerializeField] private GameObject shopItemTemplate;

        private readonly List<GameObject> activeMessageObjects = new();
        private readonly List<GameObject> activeShopItemObjects = new();
        private NPCChatAgent currentAgent;
        private PlayerInventory currentInventory;
        private PlayerWallet currentWallet;
        private bool isSending;
        private bool hasInputLock;
        private bool isRegisteredForDialogInput;

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

            CacheShopReferences();
            CacheRelationshipBarReferences();
        }

        // IDialog — CloseDialog is called by DialogInputManager when Escape is pressed.
        public void CloseDialog() => CloseConversation();

        private void OnDisable()
        {
            ReleaseDialogState(notifyAgent: true);
        }

        private void OnDestroy()
        {
            ReleaseDialogState(notifyAgent: true);
            if (Instance == this)
            {
                Instance = null;
            }
        }

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
            currentInventory = FindFirstObjectByType<PlayerInventory>();
            currentWallet = FindFirstObjectByType<PlayerWallet>();
            ClearMessages();
            ClearShopItems();
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

            RefreshRelationshipBars();

            // Lock player movement globally while the chat is open.
            if (!hasInputLock)
            {
                PlayerInputLock.Lock();
                hasInputLock = true;
            }

            if (!isRegisteredForDialogInput)
            {
                DialogInputManager.Register(this);
                isRegisteredForDialogInput = true;
            }

            RenderHistory(agent.CurrentConversation.messages);
            RefreshShop(agent);
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

        public void RefreshRelationshipBars()
        {
            CacheRelationshipBarReferences();

            var stats = currentAgent != null ? currentAgent.RelationshipStats : null;
            var hasStats = stats != null;

            if (relationshipBarsRoot != null)
            {
                relationshipBarsRoot.SetActive(hasStats);
            }

            if (!hasStats)
            {
                SetRelationshipBar(loveBarFill, 0);
                SetRelationshipBar(friendshipBarFill, 0);
                SetRelationshipBar(trustBarFill, 0);
                return;
            }

            SetRelationshipBar(loveBarFill, stats.Love);
            SetRelationshipBar(friendshipBarFill, stats.Friendship);
            SetRelationshipBar(trustBarFill, stats.Trust);
        }

        public void CloseConversation()
        {
            if (!IsOpen)
            {
                ReleaseDialogState(notifyAgent: true);
                return;
            }

            SetLoading(false, string.Empty);
            panelRoot.SetActive(false);
            ClearMessages();
            ClearShopItems();
            ToggleShopPanel(false);
            ReleaseDialogState(notifyAgent: true);
        }

        private void ReleaseDialogState(bool notifyAgent)
        {
            var agent = currentAgent;
            currentAgent = null;
            currentInventory = null;
            currentWallet = null;
            isSending = false;

            if (isRegisteredForDialogInput)
            {
                DialogInputManager.Unregister(this);
                isRegisteredForDialogInput = false;
            }

            if (hasInputLock)
            {
                PlayerInputLock.Unlock();
                hasInputLock = false;
            }

            if (notifyAgent)
            {
                agent?.EndConversation();
            }
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

        private void CacheRelationshipBarReferences()
        {
            if (panelRoot == null)
            {
                return;
            }

            var header = panelRoot.transform.Find("Header");
            var barsRootTransform = header != null ? header.Find("RelationshipBars") : null;

            if (relationshipBarsRoot == null && barsRootTransform != null)
            {
                relationshipBarsRoot = barsRootTransform.gameObject;
            }

            if (loveBarFill == null)
            {
                loveBarFill = barsRootTransform?.Find("LoveBar/Fill")?.GetComponent<Image>();
            }

            if (friendshipBarFill == null)
            {
                friendshipBarFill = barsRootTransform?.Find("FriendshipBar/Fill")?.GetComponent<Image>();
            }

            if (trustBarFill == null)
            {
                trustBarFill = barsRootTransform?.Find("TrustBar/Fill")?.GetComponent<Image>();
            }
        }

        private static void SetRelationshipBar(Image fillImage, int value)
        {
            if (fillImage == null)
            {
                return;
            }

            var normalizedValue = Mathf.Clamp01(value / 100f);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Vertical;
            fillImage.fillOrigin = (int)Image.OriginVertical.Bottom;
            fillImage.fillAmount = normalizedValue;
        }

        private void CacheShopReferences()
        {
            if (panelRoot == null)
            {
                return;
            }

            if (sellPanelRoot == null)
            {
                var sellPanelTransform = panelRoot.transform.Find("SellPanel");
                if (sellPanelTransform != null)
                {
                    sellPanelRoot = sellPanelTransform.gameObject;
                }
            }

            if (shopTitleLabel == null)
            {
                shopTitleLabel = panelRoot.transform.Find("SellPanel/ShopHeader/ShopTitle")?.GetComponent<TMP_Text>();
            }

            if (shopGoldLabel == null)
            {
                shopGoldLabel = panelRoot.transform.Find("SellPanel/ShopHeader/ShopGold")?.GetComponent<TMP_Text>();
            }

            if (shopStatusLabel == null)
            {
                shopStatusLabel = panelRoot.transform.Find("SellPanel/ShopStatus")?.GetComponent<TMP_Text>();
            }

            if (shopItemsContainer == null)
            {
                shopItemsContainer = panelRoot.transform.Find("SellPanel/ShopBody/ShopContent");
            }

            if (shopItemTemplate == null && shopItemsContainer != null)
            {
                var templateTransform = shopItemsContainer.Find("ShopItemTemplate");
                if (templateTransform != null)
                {
                    shopItemTemplate = templateTransform.gameObject;
                }
            }
        }

        private void RefreshShop(NPCChatAgent agent)
        {
            CacheShopReferences();

            var seller = agent != null ? agent.SellComponent : null;
            var merchant = agent != null ? agent.MerchantComponent : null;
            var hasMerchant = merchant != null;
            var hasShop = !hasMerchant && seller != null && seller.Stock != null && seller.Stock.Count > 0;

            ToggleShopPanel(hasMerchant || hasShop);
            if (!hasMerchant && !hasShop)
            {
                return;
            }

            if (shopTitleLabel != null)
            {
                shopTitleLabel.text = hasMerchant ? merchant.MerchantDisplayName : seller.ShopDisplayName;
            }

            RefreshGoldLabel();
            
            string merchantStatus = "Select an inventory item to sell.";
            if (merchant is DonaldMerchantComponent donald)
            {
                merchantStatus = $"Select an inventory item to sell. (Daily Limit: {donald.RemainingLimit}g)";
            }
            SetShopStatus(hasMerchant ? merchantStatus : "Select an item to buy.");

            if (hasMerchant)
            {
                RefreshMerchant(merchant);
                return;
            }

            foreach (var listing in seller.Stock)
            {
                if (listing == null || listing.item == null || shopItemsContainer == null || shopItemTemplate == null)
                {
                    continue;
                }

                var row = Instantiate(shopItemTemplate, shopItemsContainer, false);
                row.name = $"{listing.item.itemName} Row";
                row.SetActive(true);

                var icon = row.transform.Find("Icon")?.GetComponent<Image>();
                var itemName = row.transform.Find("ItemName")?.GetComponent<TMP_Text>();
                var price = row.transform.Find("Price")?.GetComponent<TMP_Text>();
                var buyButton = row.transform.Find("BuyButton")?.GetComponent<Button>();
                var buyLabel = row.transform.Find("BuyButton/Label")?.GetComponent<TMP_Text>();

                if (icon != null)
                {
                    icon.sprite = listing.item.icon;
                    icon.enabled = listing.item.icon != null;
                }

                if (itemName != null)
                {
                    itemName.text = listing.item.itemName;
                }

                if (price != null)
                {
                    price.text = $"{seller.GetPurchasePrice(listing)}\nGOLD";
                }

                if (buyLabel != null)
                {
                    buyLabel.text = "Buy";
                }

                if (buyButton != null)
                {
                    buyButton.onClick.RemoveAllListeners();
                    buyButton.onClick.AddListener(() => TryBuyListing(seller, listing));
                }

                activeShopItemObjects.Add(row);
            }
        }

        private void RefreshMerchant(NPCMerchantComponent merchant)
        {
            if (merchant == null || currentInventory == null)
            {
                return;
            }

            foreach (var stack in currentInventory.Items)
            {
                if (stack == null || !stack.IsValid || !merchant.CanSell(stack.item) || shopItemsContainer == null || shopItemTemplate == null)
                {
                    continue;
                }

                var row = Instantiate(shopItemTemplate, shopItemsContainer, false);
                row.name = $"{stack.item.itemName} Merchant Row";
                row.SetActive(true);

                var icon = row.transform.Find("Icon")?.GetComponent<Image>();
                var itemName = row.transform.Find("ItemName")?.GetComponent<TMP_Text>();
                var price = row.transform.Find("Price")?.GetComponent<TMP_Text>();
                var button = row.transform.Find("BuyButton")?.GetComponent<Button>();
                var buttonLabel = row.transform.Find("BuyButton/Label")?.GetComponent<TMP_Text>();

                if (icon != null)
                {
                    icon.sprite = stack.item.icon;
                    icon.enabled = stack.item.icon != null;
                }

                if (itemName != null)
                {
                    itemName.text = stack.quantity > 1 ? $"{stack.item.itemName} x{stack.quantity}" : stack.item.itemName;
                }

                if (price != null)
                {
                    price.text = $"{merchant.GetSellPrice(stack.item)}\nGOLD";
                }

                if (buttonLabel != null)
                {
                    buttonLabel.text = "Sell";
                }

                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => TrySellListing(merchant, stack.item));
                }

                activeShopItemObjects.Add(row);
            }

            if (activeShopItemObjects.Count == 0)
            {
                SetShopStatus("You have no sellable items for this merchant.");
            }
        }

        private void TryBuyListing(NPCSellComponent seller, NPCShopListing listing)
        {
            if (seller == null)
            {
                return;
            }

            var success = seller.TryPurchase(listing, currentInventory, currentWallet, out var resultMessage);
            SetShopStatus(resultMessage);
            RefreshGoldLabel();

            if (!success)
            {
                return;
            }
        }

        private void TrySellListing(NPCMerchantComponent merchant, ItemData item)
        {
            if (merchant == null || currentInventory == null || item == null)
            {
                return;
            }

            ItemStack matchingStack = null;
            foreach (var stack in currentInventory.Items)
            {
                if (stack != null && stack.item == item && stack.quantity > 0)
                {
                    matchingStack = stack;
                    break;
                }
            }

            if (matchingStack == null)
            {
                SetShopStatus($"You do not have any {item.itemName} left.");
                RefreshMerchant(merchant);
                return;
            }

            var success = merchant.TrySell(matchingStack, currentInventory, currentWallet, out var resultMessage);
            SetShopStatus(resultMessage);
            RefreshGoldLabel();

            if (!success)
            {
                return;
            }

            ClearShopItems();
            RefreshMerchant(merchant);
        }

        private void RefreshGoldLabel()
        {
            if (shopGoldLabel == null)
            {
                return;
            }

            shopGoldLabel.text = currentWallet != null ? $"Gold: {currentWallet.CurrentGold}" : "Gold: --";
        }

        private void SetShopStatus(string message)
        {
            if (shopStatusLabel != null)
            {
                shopStatusLabel.text = message;
            }
        }

        private void ClearShopItems()
        {
            foreach (var row in activeShopItemObjects)
            {
                if (row != null)
                {
                    Destroy(row);
                }
            }

            activeShopItemObjects.Clear();
        }

        private void ToggleShopPanel(bool visible)
        {
            if (sellPanelRoot != null)
            {
                sellPanelRoot.SetActive(visible);
            }
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
