using System.IO;
using LLMValley.NPCChat;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace LLMValley.Editor
{
    public static class NPCChatSceneBootstrap
    {
        [MenuItem("Tools/LLM Valley/Setup NPC Chat Sample Scene")]
        public static void SetupSampleScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid())
            {
                Debug.LogError("No active scene is loaded.");
                return;
            }

            var persona = EnsurePersonaAsset();

            var systems = GameObject.Find("NPC Chat Systems") ?? new GameObject("NPC Chat Systems");
            var inputLocker = EnsureComponent<NPCChatInputLocker>(systems);
            var uiManager = EnsureComponent<NPCChatUIManager>(systems);
            EnsureComponent<NPCOpenRouterClient>(systems);

            EnsureEventSystem();
            var canvas = EnsureCanvas();
            var uiRefs = BuildChatPanel(canvas.gameObject);
            AssignUIManager(uiManager, inputLocker, uiRefs);

            var playerInput = EnsureSamplePlayer();
            AssignInputLocker(inputLocker, playerInput);
            EnsureSampleNpc(persona);

            EditorSceneManager.MarkSceneDirty(scene);
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"NPC chat sample setup complete in scene '{scene.name}'.");
        }

        private static NPCPersona EnsurePersonaAsset()
        {
            const string personaDirectory = "Assets/Scriptables/NPC";
            const string personaPath = personaDirectory + "/SampleBlacksmithPersona.asset";

            if (!Directory.Exists(personaDirectory))
            {
                Directory.CreateDirectory(personaDirectory);
                AssetDatabase.Refresh();
            }

            var persona = AssetDatabase.LoadAssetAtPath<NPCPersona>(personaPath);
            if (persona != null)
            {
                return persona;
            }

            persona = ScriptableObject.CreateInstance<NPCPersona>();
            var serializedPersona = new SerializedObject(persona);
            serializedPersona.FindProperty("npcId").stringValue = "sample-blacksmith";
            serializedPersona.FindProperty("displayName").stringValue = "Torak the Blacksmith";
            serializedPersona.FindProperty("systemPrompt").stringValue =
                "You are Torak, a grumpy but capable village blacksmith. Stay in character, keep answers concise, and respond like an NPC in a fantasy village.";
            serializedPersona.FindProperty("openingLine").stringValue =
                "What do you want? If it's about a blade, speak clearly.";
            serializedPersona.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(persona, personaPath);
            return persona;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        private static Canvas EnsureCanvas()
        {
            var canvasObject = GameObject.Find("NPC Chat Canvas") ??
                               new GameObject(
                                   "NPC Chat Canvas",
                                   typeof(RectTransform),
                                   typeof(Canvas),
                                   typeof(CanvasScaler),
                                   typeof(GraphicRaycaster));

            var canvas = EnsureComponent<Canvas>(canvasObject);
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = EnsureComponent<CanvasScaler>(canvasObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static NPCChatUIRefs BuildChatPanel(GameObject canvas)
        {
            var panel = EnsureChild(canvas, "NPCChatPanel");
            Stretch(panel.GetComponent<RectTransform>());

            var panelImage = EnsureComponent<Image>(panel);
            panelImage.color = new Color(0.95f, 0.95f, 0.96f, 0.98f);

            var panelLayout = EnsureComponent<VerticalLayoutGroup>(panel);
            panelLayout.padding = new RectOffset(40, 40, 26, 28);
            panelLayout.spacing = 14f;
            panelLayout.childControlHeight = true;
            panelLayout.childControlWidth = true;
            panelLayout.childForceExpandHeight = false;
            panelLayout.childForceExpandWidth = true;

            var header = EnsureChild(panel, "Header");
            var headerLayout = EnsureComponent<HorizontalLayoutGroup>(header);
            headerLayout.spacing = 18f;
            headerLayout.childAlignment = TextAnchor.MiddleLeft;
            headerLayout.childForceExpandHeight = false;
            headerLayout.childForceExpandWidth = false;
            EnsureComponent<LayoutElement>(header).preferredHeight = 110f;

            var portraitObject = EnsureChild(header, "Portrait");
            portraitObject.GetComponent<RectTransform>().sizeDelta = new Vector2(72f, 72f);
            var portraitImage = EnsureComponent<Image>(portraitObject);
            portraitImage.color = new Color(1f, 1f, 1f, 0.92f);

            var titleWrap = EnsureChild(header, "TitleWrap");
            var titleWrapLayout = EnsureComponent<VerticalLayoutGroup>(titleWrap);
            titleWrapLayout.spacing = 6f;
            titleWrapLayout.childAlignment = TextAnchor.MiddleLeft;
            titleWrapLayout.childForceExpandHeight = false;
            titleWrapLayout.childForceExpandWidth = true;
            EnsureComponent<LayoutElement>(titleWrap).flexibleWidth = 1f;

            var titleLabel = CreateLabel(titleWrap, "Title", "NPC", 28f, FontStyles.Bold, TextAlignmentOptions.Left);
            titleLabel.color = new Color(0.11f, 0.13f, 0.17f, 1f);
            var statusLabel = CreateLabel(titleWrap, "Status", "Talk to the NPC.", 18f, FontStyles.Normal, TextAlignmentOptions.Left);
            statusLabel.color = new Color(0.43f, 0.47f, 0.54f, 1f);

            var closeButton = CreateButton(header, "CloseButton", "Close");
            closeButton.GetComponent<RectTransform>().sizeDelta = new Vector2(156f, 60f);

            var body = EnsureChild(panel, "Body");
            EnsureComponent<LayoutElement>(body).flexibleHeight = 1f;
            EnsureComponent<Image>(body).color = new Color(0.93f, 0.93f, 0.94f, 1f);

            var scrollRect = EnsureComponent<ScrollRect>(body);
            scrollRect.horizontal = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.viewport = body.GetComponent<RectTransform>();

            var existingViewport = body.transform.Find("Viewport");
            if (existingViewport != null)
            {
                Object.DestroyImmediate(existingViewport.gameObject);
            }

            var content = EnsureChild(body, "Content");
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var contentLayout = EnsureComponent<VerticalLayoutGroup>(content);
            contentLayout.spacing = 18f;
            contentLayout.padding = new RectOffset(12, 12, 16, 16);
            contentLayout.childAlignment = TextAnchor.UpperLeft;
            contentLayout.childControlHeight = true;
            contentLayout.childControlWidth = true;
            contentLayout.childForceExpandHeight = false;
            contentLayout.childForceExpandWidth = true;

            EnsureComponent<ContentSizeFitter>(content).verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scrollRect.content = contentRect;

            var userMessageTemplate = BuildMessageTemplate(content, "My Message", true);
            var assistantMessageTemplate = BuildMessageTemplate(content, "Chat Response", false);

            var footer = EnsureChild(panel, "Footer");
            var footerLayout = EnsureComponent<HorizontalLayoutGroup>(footer);
            footerLayout.spacing = 18f;
            footerLayout.childAlignment = TextAnchor.MiddleLeft;
            footerLayout.childForceExpandHeight = false;
            footerLayout.childForceExpandWidth = false;
            EnsureComponent<LayoutElement>(footer).preferredHeight = 96f;

            var inputObject = EnsureChild(footer, "InputField");
            EnsureComponent<LayoutElement>(inputObject).flexibleWidth = 1f;
            EnsureComponent<Image>(inputObject).color = Color.white;
            var inputField = EnsureComponent<TMP_InputField>(inputObject);
            inputObject.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 76f);

            var textArea = EnsureChild(inputObject, "Text Area");
            Stretch(textArea.GetComponent<RectTransform>());
            var placeholder = CreateLabel(
                textArea,
                "Placeholder",
                "Write a message to the NPC...",
                20f,
                FontStyles.Italic,
                TextAlignmentOptions.Left);
            placeholder.color = new Color(0.44f, 0.47f, 0.55f, 0.8f);
            var inputText = CreateLabel(textArea, "Text", string.Empty, 20f, FontStyles.Normal, TextAlignmentOptions.Left);
            inputText.color = new Color(0.14f, 0.16f, 0.20f, 1f);

            var serializedInput = new SerializedObject(inputField);
            serializedInput.FindProperty("m_TextViewport").objectReferenceValue = textArea.GetComponent<RectTransform>();
            serializedInput.FindProperty("m_TextComponent").objectReferenceValue = inputText;
            serializedInput.FindProperty("m_Placeholder").objectReferenceValue = placeholder;
            serializedInput.FindProperty("m_LineType").enumValueIndex = 0;
            serializedInput.ApplyModifiedPropertiesWithoutUndo();

            var sendButton = CreateButton(footer, "SendButton", "Send");
            sendButton.GetComponent<RectTransform>().sizeDelta = new Vector2(156f, 76f);

            var loadingIndicator = EnsureChild(footer, "LoadingIndicator");
            var loadingLabel = CreateLabel(
                loadingIndicator,
                "LoadingLabel",
                "NPC is thinking...",
                20f,
                FontStyles.Italic,
                TextAlignmentOptions.Left);
            loadingLabel.color = new Color(0.30f, 0.36f, 0.88f, 1f);
            loadingIndicator.SetActive(false);
            panel.SetActive(false);

            return new NPCChatUIRefs
            {
                Panel = panel,
                TitleLabel = titleLabel,
                StatusLabel = statusLabel,
                PortraitImage = portraitImage,
                HistoryScrollRect = scrollRect,
                MessageContainer = content.transform,
                UserMessageTemplate = userMessageTemplate,
                AssistantMessageTemplate = assistantMessageTemplate,
                InputField = inputField,
                SendButton = sendButton,
                CloseButton = closeButton,
                LoadingIndicator = loadingIndicator
            };
        }

        private static TMP_Text BuildMessageTemplate(GameObject parent, string templateName, bool isUserTemplate)
        {
            var messageTemplate = EnsureChild(parent, templateName);
            messageTemplate.SetActive(false);

            var templateText = EnsureComponent<TextMeshProUGUI>(messageTemplate);
            templateText.text = isUserTemplate ? "My message" : "Chat response";
            templateText.fontSize = 22f;
            templateText.textWrappingMode = TextWrappingModes.Normal;
            templateText.color = isUserTemplate ? Color.white : new Color(0.14f, 0.16f, 0.20f, 1f);
            templateText.alignment = isUserTemplate ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;

            return templateText;
        }

        private static void AssignUIManager(
            NPCChatUIManager uiManager,
            NPCChatInputLocker inputLocker,
            NPCChatUIRefs refs)
        {
            var serializedUI = new SerializedObject(uiManager);
            serializedUI.FindProperty("panelRoot").objectReferenceValue = refs.Panel;
            serializedUI.FindProperty("titleLabel").objectReferenceValue = refs.TitleLabel;
            serializedUI.FindProperty("statusLabel").objectReferenceValue = refs.StatusLabel;
            serializedUI.FindProperty("portraitImage").objectReferenceValue = refs.PortraitImage;
            serializedUI.FindProperty("historyScrollRect").objectReferenceValue = refs.HistoryScrollRect;
            serializedUI.FindProperty("messageContainer").objectReferenceValue = refs.MessageContainer;
            serializedUI.FindProperty("userMessageTemplate").objectReferenceValue = refs.UserMessageTemplate;
            serializedUI.FindProperty("assistantMessageTemplate").objectReferenceValue = refs.AssistantMessageTemplate;
            serializedUI.FindProperty("inputField").objectReferenceValue = refs.InputField;
            serializedUI.FindProperty("sendButton").objectReferenceValue = refs.SendButton;
            serializedUI.FindProperty("closeButton").objectReferenceValue = refs.CloseButton;
            serializedUI.FindProperty("loadingIndicator").objectReferenceValue = refs.LoadingIndicator;
            serializedUI.FindProperty("inputLocker").objectReferenceValue = inputLocker;
            serializedUI.ApplyModifiedPropertiesWithoutUndo();
        }

        private static PlayerInput EnsureSamplePlayer()
        {
            var player = GameObject.Find("Sample Player") ?? GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Sample Player";
            player.transform.position = new Vector3(0f, 1f, -1.5f);

            var rigidbody = EnsureComponent<Rigidbody>(player);
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;

            var playerInput = EnsureComponent<PlayerInput>(player);
            playerInput.actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
            playerInput.defaultActionMap = "Player";
            playerInput.notificationBehavior = PlayerNotifications.InvokeUnityEvents;

            return playerInput;
        }

        private static void AssignInputLocker(NPCChatInputLocker inputLocker, PlayerInput playerInput)
        {
            var serializedLocker = new SerializedObject(inputLocker);
            serializedLocker.FindProperty("playerInput").objectReferenceValue = playerInput;
            serializedLocker.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void EnsureSampleNpc(NPCPersona persona)
        {
            var npc = GameObject.Find("Sample NPC") ?? GameObject.CreatePrimitive(PrimitiveType.Cube);
            npc.name = "Sample NPC";
            npc.transform.position = new Vector3(0f, 0.75f, 1.5f);
            npc.transform.localScale = new Vector3(1.2f, 1.5f, 1.2f);

            var renderer = npc.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial.color = new Color(0.55f, 0.32f, 0.18f, 1f);
            }

            var collider = EnsureComponent<BoxCollider>(npc);
            collider.isTrigger = true;
            collider.size = new Vector3(4f, 3f, 4f);

            var visual = EnsureChild(npc, "Visual");
            visual.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = Vector3.one;
            var spriteRenderer = EnsureComponent<SpriteRenderer>(visual);
            spriteRenderer.sortingOrder = 10;

            var agent = EnsureComponent<NPCChatAgent>(npc);
            var serializedAgent = new SerializedObject(agent);
            serializedAgent.FindProperty("apiKey").stringValue = string.Empty;
            serializedAgent.FindProperty("selectedModelId").stringValue = "openai/gpt-oss-20b:free";
            serializedAgent.FindProperty("allowFreeModelsOnly").boolValue = true;
            serializedAgent.FindProperty("persona").objectReferenceValue = persona;
            serializedAgent.FindProperty("conversationSaveId").stringValue = "sample-blacksmith";
            serializedAgent.FindProperty("interactionRadius").floatValue = 4f;
            serializedAgent.FindProperty("interactionOrigin").objectReferenceValue = npc.transform;
            serializedAgent.FindProperty("worldSpriteRenderer").objectReferenceValue = spriteRenderer;
            serializedAgent.FindProperty("worldSpriteSortingOrder").intValue = 10;
            serializedAgent.FindProperty("hideMeshRendererWhenUsingSprite").boolValue = true;
            serializedAgent.FindProperty("interactionTrigger").objectReferenceValue = collider;
            serializedAgent.ApplyModifiedPropertiesWithoutUndo();

            agent.RefreshVisualFromPersona();
        }

        private static GameObject EnsureChild(GameObject parent, string childName)
        {
            var child = parent.transform.Find(childName);
            if (child != null)
            {
                return child.gameObject;
            }

            var gameObject = new GameObject(childName, typeof(RectTransform));
            gameObject.transform.SetParent(parent.transform, false);
            return gameObject;
        }

        private static T EnsureComponent<T>(GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static TMP_Text CreateLabel(
            GameObject parent,
            string name,
            string text,
            float fontSize,
            FontStyles styles,
            TextAlignmentOptions alignment)
        {
            var labelObject = new GameObject(name, typeof(RectTransform));
            labelObject.transform.SetParent(parent.transform, false);
            var label = labelObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.fontStyle = styles;
            label.alignment = alignment;
            label.color = Color.white;
            return label;
        }

        private static Button CreateButton(GameObject parent, string name, string text)
        {
            var buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent.transform, false);

            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.18f, 0.5f, 0.9f, 0.95f);

            var button = buttonObject.GetComponent<Button>();
            button.targetGraphic = image;

            var label = CreateLabel(buttonObject, "Label", text, 24f, FontStyles.Bold, TextAlignmentOptions.Center);
            Stretch(label.rectTransform);

            return button;
        }

        private class NPCChatUIRefs
        {
            public GameObject Panel;
            public TMP_Text TitleLabel;
            public TMP_Text StatusLabel;
            public Image PortraitImage;
            public ScrollRect HistoryScrollRect;
            public Transform MessageContainer;
            public TMP_Text UserMessageTemplate;
            public TMP_Text AssistantMessageTemplate;
            public TMP_InputField InputField;
            public Button SendButton;
            public Button CloseButton;
            public GameObject LoadingIndicator;
        }
    }
}
