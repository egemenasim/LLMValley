using UnityEngine;

namespace LLMValley.NPCChat
{
    [CreateAssetMenu(fileName = "NPCPersona", menuName = "LLM Valley/NPC Chat Persona")]
    public class NPCPersona : ScriptableObject
    {
        [SerializeField] private string npcId = "npc";
        [SerializeField] private string displayName = "NPC";
        [SerializeField, TextArea(5, 12)] private string systemPrompt =
            "You are a helpful NPC. Stay in character and answer briefly.";
        [SerializeField, TextArea(2, 6)] private string openingLine = string.Empty;
        [SerializeField] private Sprite portrait;

        public string NpcId => npcId;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? name : displayName;
        public string SystemPrompt => systemPrompt;
        public string OpeningLine => openingLine;
        public Sprite Portrait => portrait;
    }
}
