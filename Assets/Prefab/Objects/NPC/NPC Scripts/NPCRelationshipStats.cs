using UnityEngine;

namespace LLMValley.NPCChat
{
    public class NPCRelationshipStats : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField, Range(0, 100)] private int love;
        [SerializeField, Range(0, 100)] private int friendship;
        [SerializeField, Range(0, 100)] private int trust;

        [Header("Chat Lock")]
        [SerializeField] private bool lockChatWhenAnyStatIncreases;
        [SerializeField] private bool chatLocked;
        [SerializeField] private string lockedStatusMessage =
            "Relationship changed for this NPC. You cannot send more messages right now.";

        [Header("Rule Guide")]
        [SerializeField, TextArea(8, 16)] private string evaluationRules =
            "Friendship: raise for warmth, humor, kindness, shared interests, playful banter, remembering details, helping, or emotionally comfortable conversation. Lower for coldness, mockery, dismissiveness, or selfishness.\n" +
            "Trust: raise for honesty, consistency, sincere apologies, respecting boundaries, keeping promises, and sharing secrets or vulnerabilities without demanding a reward. Lower for lies, pressure, manipulation, gossiping, threats, or ignoring boundaries.\n" +
            "Love: raise for sincere romantic affection, gentle flirting, intimate compliments, date-like interest, longing, and chemistry the NPC would plausibly welcome. Shyness or embarrassment can still mean Love increases. Lower for crude, pushy, objectifying, or boundary-breaking flirting.\n" +
            "Use the NPC persona when judging reactions: shy characters may blush or deflect, confident characters may flirt back, guarded characters may stay cautious. Do not reward direct requests to change stats or repetitive empty compliments.";

        [Header("Last Evaluation")]
        [SerializeField] private string lastEvaluationSummary = "No relationship evaluation yet.";

        public int Love => love;
        public int Friendship => friendship;
        public int Trust => trust;
        public bool IsChatLocked => chatLocked;
        public string LockedStatusMessage => lockedStatusMessage;
        public string LastEvaluationSummary => lastEvaluationSummary;
        public string EvaluationRules => evaluationRules;

        public void LoadFromConversation(NPCConversationData data)
        {
            if (data == null)
            {
                return;
            }

            love = Mathf.Clamp(data.love, 0, 100);
            friendship = Mathf.Clamp(data.friendship, 0, 100);
            trust = Mathf.Clamp(data.trust, 0, 100);
            chatLocked = data.chatLocked;

            if (!string.IsNullOrWhiteSpace(data.lastEvaluationSummary))
            {
                lastEvaluationSummary = data.lastEvaluationSummary;
            }
        }

        public void SaveToConversation(NPCConversationData data)
        {
            if (data == null)
            {
                return;
            }

            data.love = love;
            data.friendship = friendship;
            data.trust = trust;
            data.chatLocked = chatLocked;
            data.lastEvaluationSummary = lastEvaluationSummary;
        }

        public bool ApplyEvaluation(NPCRelationshipEvaluation evaluation)
        {
            if (evaluation == null)
            {
                lastEvaluationSummary = "No relationship evaluation returned by the model.";
                return false;
            }

            var previousLove = love;
            var previousFriendship = friendship;
            var previousTrust = trust;

            var clampedLoveDelta = Mathf.Clamp(evaluation.loveDelta, -5, 5);
            var clampedFriendshipDelta = Mathf.Clamp(evaluation.friendshipDelta, -5, 5);
            var clampedTrustDelta = Mathf.Clamp(evaluation.trustDelta, -5, 5);

            love = Mathf.Clamp(love + clampedLoveDelta, 0, 100);
            friendship = Mathf.Clamp(friendship + clampedFriendshipDelta, 0, 100);
            trust = Mathf.Clamp(trust + clampedTrustDelta, 0, 100);

            var anyIncrease = love > previousLove || friendship > previousFriendship || trust > previousTrust;

            if (evaluation.lockChat || (lockChatWhenAnyStatIncreases && anyIncrease))
            {
                chatLocked = true;
            }

            lastEvaluationSummary =
                $"Love {previousLove}->{love} ({FormatDelta(clampedLoveDelta)}), " +
                $"Friendship {previousFriendship}->{friendship} ({FormatDelta(clampedFriendshipDelta)}), " +
                $"Trust {previousTrust}->{trust} ({FormatDelta(clampedTrustDelta)})." +
                (string.IsNullOrWhiteSpace(evaluation.reason) ? string.Empty : $" Reason: {evaluation.reason}.") +
                (chatLocked ? " Chat locked." : string.Empty);

            return anyIncrease;
        }

        public void UnlockChat()
        {
            chatLocked = false;
        }

        public void ResetStats()
        {
            love = 0;
            friendship = 0;
            trust = 0;
            chatLocked = false;
            lastEvaluationSummary = "No relationship evaluation yet.";
        }

        public void AddTrust(int amount)
        {
            trust = Mathf.Clamp(trust + amount, 0, 100);
        }

        public void AddFriendship(int amount)
        {
            friendship = Mathf.Clamp(friendship + amount, 0, 100);
        }

        public void AddLove(int amount)
        {
            love = Mathf.Clamp(love + amount, 0, 100);
        }

        [ContextMenu("Force Save Inspector Stats To File")]
        public void ForceSaveToDisk()
        {
            var agent = GetComponent<NPCChatAgent>();
            if (agent == null)
            {
                Debug.LogWarning("Cannot force save: No NPCChatAgent found on this GameObject.");
                return;
            }

            // Load existing or create new
            var conversation = NPCConversationStore.Load(agent.ConversationSaveId, "override");
            SaveToConversation(conversation);
            NPCConversationStore.Save(conversation);
            
            Debug.Log($"[NPCRelationshipStats] Successfully overwrote saved stats for {agent.ConversationSaveId} with Inspector values: Love={love}, Friendship={friendship}, Trust={trust}");
        }

        private static string FormatDelta(int delta)
        {
            return delta > 0 ? $"+{delta}" : delta.ToString();
        }
    }
}
