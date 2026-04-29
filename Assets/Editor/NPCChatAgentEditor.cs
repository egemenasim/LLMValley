using LLMValley.NPCChat;
using UnityEditor;
using UnityEngine;

namespace LLMValley.Editor
{
    [InitializeOnLoad]
    public static class NPCRelationshipStatsPlayModePersistence
    {
        static NPCRelationshipStatsPlayModePersistence()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            foreach (var agent in Object.FindObjectsByType<NPCChatAgent>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (agent == null || !agent.LoadRelationshipProgress())
                {
                    continue;
                }

                var stats = agent.GetComponent<NPCRelationshipStats>();
                if (stats != null)
                {
                    EditorUtility.SetDirty(stats);
                }

                EditorUtility.SetDirty(agent);
            }
        }
    }

    [CustomEditor(typeof(NPCChatAgent))]
    public class NPCChatAgentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(12f);

            var agent = (NPCChatAgent)target;

            EditorGUILayout.LabelField("Conversation Tools", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                $"Save Id: {agent.ConversationSaveId}\nPath: {agent.ConversationSavePath}",
                MessageType.Info);

            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                if (GUILayout.Button("Clear Chat History"))
                {
                    var deleted = agent.ClearConversationHistory();
                    EditorUtility.SetDirty(agent);
                    AssetDatabase.SaveAssets();

                    Debug.Log(
                        deleted
                            ? $"[NPC Chat] Cleared chat history for '{agent.name}'. Relationship stats were kept."
                            : $"[NPC Chat] No saved chat history found for '{agent.name}'.",
                        agent);
                }
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Relationship Tools", EditorStyles.boldLabel);
            if (GUILayout.Button("Reset Relationship Stats"))
            {
                agent.ResetRelationshipProgress();

                var stats = agent.GetComponent<NPCRelationshipStats>();
                if (stats != null)
                {
                    EditorUtility.SetDirty(stats);
                }

                EditorUtility.SetDirty(agent);
                AssetDatabase.SaveAssets();

                Debug.Log($"[NPC Chat] Reset relationship stats for '{agent.name}'.", agent);
            }

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Clear Chat History keeps saved relationship stats. Use Reset Relationship Stats only when you intentionally want Love, Friendship, Trust, and chat lock reset.",
                    MessageType.None);
            }
        }
    }
}
