using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LLMValley.NPCChat
{
    [RequireComponent(typeof(Collider))]
    public class NPCChatAgent : MonoBehaviour
    {
        private const string PlayerTag = "Player";

        [Header("OpenRouter")]
        [SerializeField] private string apiKey;
        [SerializeField] private string selectedModelId = "openai/gpt-oss-20b:free";
        [SerializeField] private bool allowFreeModelsOnly = true;
        [SerializeField] private bool enableConsoleDebug = true;

        [Header("NPC")]
        [SerializeField] private NPCPersona persona;
        [SerializeField] private string conversationSaveId = string.Empty;
        [SerializeField] private float interactionRadius = 3f;
        [SerializeField] private Transform interactionOrigin;
        [SerializeField] private SpriteRenderer worldSpriteRenderer;
        [SerializeField] private int worldSpriteSortingOrder = 10;
        [SerializeField] private bool hideMeshRendererWhenUsingSprite = true;
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private TMP_Text interactionPromptLabel;
        [SerializeField] private string interactionPromptText = "Press \"E\" to interact";

        [Header("Detection")]
        [SerializeField] private Collider interactionTrigger;

        private NPCConversationData conversation;
        private Transform nearbyPlayer;
        private bool requestInFlight;
        private InputAction interactionAction;

        public NPCConversationData CurrentConversation => conversation;
        public string PersonaDisplayName => persona != null ? persona.DisplayName : name;
        public Sprite PersonaPortrait => persona != null ? persona.Portrait : null;
        public string ConversationSaveId => ResolveConversationSaveId();
        public string ConversationSavePath => NPCConversationStore.GetPath(ResolveConversationSaveId());

        private void Awake()
        {
            interactionOrigin ??= transform;
            interactionTrigger ??= GetComponent<Collider>();

            if (interactionTrigger != null)
            {
                interactionTrigger.isTrigger = true;
            }

            RefreshVisualFromPersona();
            RefreshPromptReferences();
            UpdateInteractionPrompt(false);
        }

        private void OnValidate()
        {
            interactionOrigin ??= transform;
            interactionTrigger ??= GetComponent<Collider>();

            if (worldSpriteRenderer == null)
            {
                worldSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            RefreshVisualFromPersona();
            RefreshPromptReferences();
            ApplyInteractionPromptText();
        }

        private void Update()
        {
            var uiIsOpen = NPCChatUIManager.FindExisting()?.IsOpen == true;
            if (uiIsOpen)
            {
                UpdateInteractionPrompt(false);
                return;
            }

            var isPlayerInRange = IsPlayerInRange();
            UpdateInteractionPrompt(isPlayerInRange);

            if (!isPlayerInRange)
            {
                return;
            }

            if (WasInteractPressed())
            {
                BeginConversation();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsPlayerCollider(other))
            {
                nearbyPlayer = other.transform;
                CacheInteractionAction(other);
                UpdateInteractionPrompt(IsPlayerInRange());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (nearbyPlayer == other.transform)
            {
                nearbyPlayer = null;
                interactionAction = null;
                UpdateInteractionPrompt(false);
            }
        }

        public void BeginConversation()
        {
            if (persona == null)
            {
                Debug.LogWarning($"NPC Chat Agent on '{name}' is missing a persona.");
                return;
            }

            var uiManager = NPCChatUIManager.FindExisting();
            if (uiManager == null)
            {
                Debug.LogWarning("NPCChatUIManager could not be found in the scene.");
                return;
            }

            conversation = NPCConversationStore.Load(ResolveConversationSaveId(), selectedModelId);

            if (conversation.messages.Count == 0 && !string.IsNullOrWhiteSpace(persona.OpeningLine))
            {
                conversation.messages.Add(new NPCChatMessage("assistant", persona.OpeningLine));
                NPCConversationStore.Save(conversation);
            }

            LogDebug(
                $"Conversation opened.\nNPC: {PersonaDisplayName}\nModel: {selectedModelId}\nSaved messages: {conversation.messages.Count}\nSave file: {NPCConversationStore.GetPath(ResolveConversationSaveId())}");

            UpdateInteractionPrompt(false);
            uiManager.OpenConversation(this);
        }

        public void EndConversation()
        {
            requestInFlight = false;
            UpdateInteractionPrompt(IsPlayerInRange());
        }

        public bool ClearConversationHistory()
        {
            requestInFlight = false;
            conversation = null;

            var deleted = NPCConversationStore.Delete(ResolveConversationSaveId());
            LogDebug(
                deleted
                    ? $"Conversation history deleted.\nPath: {ConversationSavePath}"
                    : $"Conversation history was already empty.\nPath: {ConversationSavePath}");

            var uiManager = NPCChatUIManager.FindExisting();
            if (uiManager != null && uiManager.CurrentAgent == this)
            {
                uiManager.RenderHistory(System.Array.Empty<NPCChatMessage>());
                uiManager.SetLoading(false, "Conversation history cleared.");
            }

            return deleted;
        }

        public void RefreshVisualFromPersona()
        {
            if (worldSpriteRenderer == null)
            {
                return;
            }

            var sprite = persona != null ? persona.WorldSprite : null;
            worldSpriteRenderer.sprite = sprite;
            worldSpriteRenderer.sortingOrder = worldSpriteSortingOrder;
            worldSpriteRenderer.enabled = sprite != null;

            if (!hideMeshRendererWhenUsingSprite)
            {
                return;
            }

            var meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = sprite == null;
            }
        }

        public void RefreshPromptReferences()
        {
            if (interactionPrompt == null)
            {
                var promptTransform = transform.Find("Interaction Prompt");
                if (promptTransform != null)
                {
                    interactionPrompt = promptTransform.gameObject;
                }
            }

            if (interactionPromptLabel == null && interactionPrompt != null)
            {
                interactionPromptLabel = interactionPrompt.GetComponentInChildren<TMP_Text>(true);
            }

            ApplyInteractionPromptText();
        }

        public void SendPlayerMessage(string text)
        {
            var uiManager = NPCChatUIManager.FindExisting();
            if (uiManager == null)
            {
                return;
            }

            if (requestInFlight)
            {
                uiManager.ShowError("Wait for the current reply to finish.");
                return;
            }

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                uiManager.ShowError("OpenRouter API key is empty on this NPC.");
                return;
            }

            if (persona == null)
            {
                uiManager.ShowError("NPC persona is not assigned.");
                return;
            }

            conversation ??= NPCConversationStore.Load(ResolveConversationSaveId(), selectedModelId);

            var userMessage = new NPCChatMessage("user", text);
            conversation.modelId = selectedModelId;
            conversation.messages.Add(userMessage);
            NPCConversationStore.Save(conversation);
            uiManager.AppendMessage(userMessage);
            uiManager.SetLoading(true, "NPC is thinking...");
            LogDebug($"User message queued:\n{text}");

            requestInFlight = true;

            NPCOpenRouterClient.GetOrCreate().SendChatCompletion(
                apiKey,
                selectedModelId,
                persona.SystemPrompt,
                conversation.messages,
                allowFreeModelsOnly,
                HandleChatResponse);
        }

        private void HandleChatResponse(string content, string error)
        {
            requestInFlight = false;

            var uiManager = NPCChatUIManager.FindExisting();
            if (uiManager == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                LogDebug($"Assistant request failed:\n{error}");
                uiManager.ShowError(error);
                return;
            }

            var assistantMessage = new NPCChatMessage("assistant", content);
            conversation.messages.Add(assistantMessage);
            NPCConversationStore.Save(conversation);
            LogDebug($"Assistant message received:\n{content}");

            if (uiManager.CurrentAgent == this)
            {
                uiManager.AppendMessage(assistantMessage);
                uiManager.SetLoading(false, "Talk to the NPC.");
            }
        }

        private bool WasInteractPressed()
        {
            if (interactionAction != null)
            {
                return interactionAction.WasPressedThisFrame();
            }

            return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        }

        private bool IsPlayerInRange()
        {
            if (nearbyPlayer == null)
            {
                nearbyPlayer = FindPlayerTransform();
            }

            if (nearbyPlayer == null)
            {
                return false;
            }

            return Vector3.Distance(interactionOrigin.position, nearbyPlayer.position) <= interactionRadius;
        }

        private Transform FindPlayerTransform()
        {
            var playerObject = GameObject.FindGameObjectWithTag(PlayerTag);
            if (playerObject != null)
            {
                CacheInteractionAction(playerObject.GetComponent<Collider>());
                return playerObject.transform;
            }

            return null;
        }

        private bool IsPlayerCollider(Collider other)
        {
            return other != null && other.CompareTag(PlayerTag);
        }

        private void CacheInteractionAction(Collider sourceCollider)
        {
            if (sourceCollider == null)
            {
                return;
            }

            var playerInput = sourceCollider.GetComponentInParent<PlayerInput>();
            if (playerInput == null || playerInput.actions == null)
            {
                return;
            }

            interactionAction = playerInput.actions.FindAction("Interact", throwIfNotFound: false);
        }

        private void UpdateInteractionPrompt(bool shouldShow)
        {
            if (interactionPrompt == null)
            {
                return;
            }

            interactionPrompt.SetActive(shouldShow);
            ApplyInteractionPromptText();
        }

        private void ApplyInteractionPromptText()
        {
            if (interactionPromptLabel != null)
            {
                interactionPromptLabel.text = interactionPromptText;
            }
        }

        private string ResolveConversationSaveId()
        {
            if (!string.IsNullOrWhiteSpace(conversationSaveId))
            {
                return conversationSaveId;
            }

            if (persona != null && !string.IsNullOrWhiteSpace(persona.NpcId))
            {
                return persona.NpcId;
            }

            return gameObject.name;
        }

        private void LogDebug(string message)
        {
            if (!enableConsoleDebug)
            {
                return;
            }

            Debug.Log($"[NPC Chat][{name}] {message}", this);
        }
    }
}
