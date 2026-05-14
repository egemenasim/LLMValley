using UnityEngine;

namespace LLMValley.NPCChat
{
    public static class OpenRouterApiKeyStore
    {
        private const string ApiKeyPlayerPrefsKey = "OpenRouterApiKey";

        public static string ApiKey => PlayerPrefs.GetString(ApiKeyPlayerPrefsKey, string.Empty);
        public static bool HasApiKey => !string.IsNullOrWhiteSpace(ApiKey);

        public static void Save(string apiKey)
        {
            PlayerPrefs.SetString(ApiKeyPlayerPrefsKey, apiKey?.Trim() ?? string.Empty);
            PlayerPrefs.Save();
        }
    }
}
