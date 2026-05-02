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
        public string miniChatHistory;
        public int lastEvaluatedUserMessageCount;
        public int love;
        public int friendship;
        public int trust;
        public bool chatLocked;
        public string lastEvaluationSummary;
    }

    [Serializable]
    public class NPCOpenRouterModelInfo
    {
        public string Id;
        public string Name;
    }

    [Serializable]
    public class NPCRelationshipEvaluation
    {
        public int loveDelta;
        public int friendshipDelta;
        public int trustDelta;
        public bool lockChat;
        public string reason;
        public string summary;
    }
}
