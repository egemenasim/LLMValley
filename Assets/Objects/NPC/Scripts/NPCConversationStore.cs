using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace LLMValley.NPCChat
{
    public static class NPCConversationStore
    {
        private const string FolderName = "NPCChats";

        public static NPCConversationData Load(string conversationSaveId, string modelId)
        {
            var path = GetPath(conversationSaveId);
            if (!File.Exists(path))
            {
                return new NPCConversationData
                {
                    conversationSaveId = conversationSaveId,
                    modelId = modelId
                };
            }

            try
            {
                var json = File.ReadAllText(path);
                var data = JsonConvert.DeserializeObject<NPCConversationData>(json);
                if (data == null)
                {
                    throw new InvalidOperationException("Conversation data was empty.");
                }

                data.conversationSaveId = string.IsNullOrWhiteSpace(data.conversationSaveId)
                    ? conversationSaveId
                    : data.conversationSaveId;
                data.modelId = string.IsNullOrWhiteSpace(data.modelId) ? modelId : data.modelId;
                data.messages ??= new System.Collections.Generic.List<NPCChatMessage>();
                return data;
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"Failed to load NPC conversation '{conversationSaveId}'. A new conversation will be created.\n{exception}");
                return new NPCConversationData
                {
                    conversationSaveId = conversationSaveId,
                    modelId = modelId
                };
            }
        }

        public static void Save(NPCConversationData data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.conversationSaveId))
            {
                return;
            }

            var directory = GetDirectory();
            Directory.CreateDirectory(directory);

            var path = GetPath(data.conversationSaveId);
            var json = JsonConvert.SerializeObject(data, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        public static string GetPath(string conversationSaveId)
        {
            var safeFileName = SanitizeFileName(conversationSaveId);
            return Path.Combine(GetDirectory(), $"{safeFileName}.json");
        }

        public static bool Delete(string conversationSaveId)
        {
            var path = GetPath(conversationSaveId);
            if (!File.Exists(path))
            {
                return false;
            }

            File.Delete(path);
            return true;
        }

        private static string GetDirectory()
        {
            return Path.Combine(Application.persistentDataPath, FolderName);
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "npc-chat";
            }

            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value.Trim();
        }
    }
}
