using System;
using System.Collections.Generic;

namespace LLMValley.NPCChat
{
    [Serializable]
    public class NPCChatMessage
    {
        public string role;
        public string content;
        public string timestampUtc;

        public NPCChatMessage()
        {
        }

        public NPCChatMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
            timestampUtc = DateTime.UtcNow.ToString("O");
        }
    }

    [Serializable]
    public class NPCConversationData
    {
        public string conversationSaveId;
        public string modelId;
        public List<NPCChatMessage> messages = new();
    }

    [Serializable]
    public class NPCOpenRouterModelInfo
    {
        public string Id;
        public string Name;
    }
}
