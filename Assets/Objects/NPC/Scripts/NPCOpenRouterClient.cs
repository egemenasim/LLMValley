using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace LLMValley.NPCChat
{
    public class NPCOpenRouterClient : MonoBehaviour
    {
        private const string ModelsUrl = "https://openrouter.ai/api/v1/models";
        private const string ChatUrl = "https://openrouter.ai/api/v1/chat/completions";

        [SerializeField] private string httpReferer = "https://github.com/egemenasim/LLMValley";
        [SerializeField] private string titleHeader = "LLM Valley";
        [SerializeField] private bool enableConsoleDebug = true;

        private readonly List<NPCOpenRouterModelInfo> freeModels = new();
        private bool isFetchingModels;
        private string lastModelsFetchError;

        public static NPCOpenRouterClient Instance { get; private set; }

        public IReadOnlyList<NPCOpenRouterModelInfo> FreeModels => freeModels;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public static NPCOpenRouterClient GetOrCreate()
        {
            if (Instance != null)
            {
                return Instance;
            }

            var existing = FindFirstObjectByType<NPCOpenRouterClient>();
            if (existing != null)
            {
                Instance = existing;
                return existing;
            }

            var host = new GameObject(nameof(NPCOpenRouterClient));
            return host.AddComponent<NPCOpenRouterClient>();
        }

        public void FetchFreeModels(string apiKey, Action<IReadOnlyList<NPCOpenRouterModelInfo>, string> onComplete)
        {
            StartCoroutine(FetchFreeModelsCoroutine(apiKey, onComplete));
        }

        public void SendChatCompletion(
            string apiKey,
            string modelId,
            string systemPrompt,
            IReadOnlyList<NPCChatMessage> conversationHistory,
            bool requireFreeModel,
            Action<string, string> onComplete)
        {
            StartCoroutine(SendChatCompletionCoroutine(apiKey, modelId, systemPrompt, conversationHistory, requireFreeModel, onComplete));
        }

        private IEnumerator FetchFreeModelsCoroutine(
            string apiKey,
            Action<IReadOnlyList<NPCOpenRouterModelInfo>, string> onComplete)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                LogDebug("Model fetch aborted because API key is missing.");
                onComplete?.Invoke(Array.Empty<NPCOpenRouterModelInfo>(), "OpenRouter API key is missing.");
                yield break;
            }

            if (isFetchingModels)
            {
                yield return new WaitUntil(() => !isFetchingModels);
                onComplete?.Invoke(freeModels, lastModelsFetchError);
                yield break;
            }

            isFetchingModels = true;
            lastModelsFetchError = null;

            using var request = UnityWebRequest.Get(ModelsUrl);
            ApplyHeaders(request, apiKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                lastModelsFetchError = $"Model list could not be fetched: {request.error}";
                LogDebug(lastModelsFetchError);
                isFetchingModels = false;
                onComplete?.Invoke(Array.Empty<NPCOpenRouterModelInfo>(), lastModelsFetchError);
                yield break;
            }

            try
            {
                var root = JObject.Parse(request.downloadHandler.text);
                var data = root["data"] as JArray;

                freeModels.Clear();
                if (data != null)
                {
                    foreach (var modelToken in data)
                    {
                        var pricing = modelToken["pricing"];
                        if (!IsFreeTextModel(pricing))
                        {
                            continue;
                        }

                        var id = modelToken["id"]?.Value<string>();
                        if (string.IsNullOrWhiteSpace(id))
                        {
                            continue;
                        }

                        freeModels.Add(new NPCOpenRouterModelInfo
                        {
                            Id = id,
                            Name = modelToken["name"]?.Value<string>() ?? id
                        });
                    }
                }

                freeModels.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase));
                LogDebug($"Fetched {freeModels.Count} free OpenRouter model(s).");
            }
            catch (Exception exception)
            {
                lastModelsFetchError = $"Model list could not be parsed: {exception.Message}";
                LogDebug(lastModelsFetchError);
            }

            isFetchingModels = false;
            onComplete?.Invoke(freeModels, lastModelsFetchError);
        }

        private IEnumerator SendChatCompletionCoroutine(
            string apiKey,
            string modelId,
            string systemPrompt,
            IReadOnlyList<NPCChatMessage> conversationHistory,
            bool requireFreeModel,
            Action<string, string> onComplete)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                onComplete?.Invoke(null, "OpenRouter API key is missing.");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(modelId))
            {
                onComplete?.Invoke(null, "A model id is required.");
                yield break;
            }

            if (string.IsNullOrWhiteSpace(systemPrompt))
            {
                onComplete?.Invoke(null, "NPC persona prompt is empty.");
                yield break;
            }

            if (requireFreeModel)
            {
                var modelValidationFinished = false;
                string modelValidationError = null;

                yield return FetchFreeModelsCoroutine(
                    apiKey,
                    (models, error) =>
                    {
                        modelValidationFinished = true;
                        if (!string.IsNullOrWhiteSpace(error))
                        {
                            modelValidationError = error;
                            return;
                        }

                        if (models.All(model => !string.Equals(model.Id, modelId, StringComparison.OrdinalIgnoreCase)))
                        {
                            modelValidationError =
                                $"'{modelId}' is not in the current free OpenRouter model list.";
                        }
                    });

                yield return new WaitUntil(() => modelValidationFinished);

                if (!string.IsNullOrWhiteSpace(modelValidationError))
                {
                    onComplete?.Invoke(null, modelValidationError);
                    yield break;
                }
            }

            var requestBody = BuildChatRequest(modelId, systemPrompt, conversationHistory);
            var payload = JsonConvert.SerializeObject(requestBody);
            LogDebug(
                $"Sending chat completion.\nModel: {modelId}\nConversation messages: {conversationHistory.Count}\nPayload:\n{FormatJson(payload)}");

            using var request = new UnityWebRequest(ChatUrl, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payload));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            ApplyHeaders(request, apiKey);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                var detail = string.IsNullOrWhiteSpace(request.downloadHandler.text)
                    ? request.error
                    : request.downloadHandler.text;
                LogDebug($"OpenRouter request failed.\nModel: {modelId}\nDetail: {detail}");
                onComplete?.Invoke(null, $"OpenRouter request failed: {detail}");
                yield break;
            }

            try
            {
                LogDebug($"OpenRouter raw response:\n{FormatJson(request.downloadHandler.text)}");
                var root = JObject.Parse(request.downloadHandler.text);
                var content = ExtractAssistantContent(root);

                if (string.IsNullOrWhiteSpace(content))
                {
                    LogDebug("OpenRouter returned an empty assistant message.");
                    onComplete?.Invoke(null, "OpenRouter returned an empty assistant message.");
                    yield break;
                }

                LogDebug($"Assistant response extracted:\n{content.Trim()}");
                onComplete?.Invoke(content.Trim(), null);
            }
            catch (Exception exception)
            {
                LogDebug($"OpenRouter response parse error: {exception.Message}");
                onComplete?.Invoke(null, $"OpenRouter response could not be parsed: {exception.Message}");
            }
        }

        private object BuildChatRequest(
            string modelId,
            string systemPrompt,
            IReadOnlyList<NPCChatMessage> conversationHistory)
        {
            var messages = new List<object>
            {
                new
                {
                    role = "system",
                    content = systemPrompt
                }
            };

            foreach (var message in conversationHistory)
            {
                if (message == null || string.IsNullOrWhiteSpace(message.role) || string.IsNullOrWhiteSpace(message.content))
                {
                    continue;
                }

                messages.Add(new
                {
                    role = message.role,
                    content = message.content
                });
            }

            return new
            {
                model = modelId,
                messages
            };
        }

        private static string ExtractAssistantContent(JObject root)
        {
            var firstChoice = root["choices"]?.FirstOrDefault();
            var contentToken = firstChoice?["message"]?["content"];

            if (contentToken == null)
            {
                return null;
            }

            if (contentToken.Type == JTokenType.String)
            {
                return contentToken.Value<string>();
            }

            if (contentToken is JArray parts)
            {
                var builder = new StringBuilder();
                foreach (var part in parts)
                {
                    var text = part["text"]?.Value<string>() ?? part["content"]?.Value<string>();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        builder.Append(text);
                    }
                }

                return builder.ToString();
            }

            return contentToken.ToString();
        }

        private static bool IsFreeTextModel(JToken pricingToken)
        {
            if (pricingToken == null)
            {
                return false;
            }

            return IsZero(pricingToken["prompt"]) &&
                   IsZero(pricingToken["completion"]) &&
                   IsZeroOrMissing(pricingToken["request"]) &&
                   IsZeroOrMissing(pricingToken["internal_reasoning"]);
        }

        private static bool IsZeroOrMissing(JToken token)
        {
            return token == null || IsZero(token);
        }

        private static bool IsZero(JToken token)
        {
            if (token == null)
            {
                return false;
            }

            var value = token.Value<string>();
            return string.Equals(value, "0", StringComparison.Ordinal) ||
                   string.Equals(value, "0.0", StringComparison.Ordinal) ||
                   string.Equals(value, "0.000000", StringComparison.Ordinal);
        }

        private void ApplyHeaders(UnityWebRequest request, string apiKey)
        {
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            if (!string.IsNullOrWhiteSpace(httpReferer))
            {
                request.SetRequestHeader("HTTP-Referer", httpReferer);
            }

            if (!string.IsNullOrWhiteSpace(titleHeader))
            {
                request.SetRequestHeader("X-Title", titleHeader);
            }
        }

        private void LogDebug(string message)
        {
            if (!enableConsoleDebug)
            {
                return;
            }

            Debug.Log($"[NPC Chat][OpenRouter] {message}", this);
        }

        private static string FormatJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return string.Empty;
            }

            try
            {
                return JToken.Parse(json).ToString(Formatting.Indented);
            }
            catch
            {
                return json;
            }
        }
    }
}
