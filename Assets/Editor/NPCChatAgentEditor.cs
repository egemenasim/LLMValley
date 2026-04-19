using LLMValley.NPCChat;
using UnityEditor;
using UnityEngine;

namespace LLMValley.Editor
{
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
                            ? $"[NPC Chat] Cleared chat history for '{agent.name}'."
                            : $"[NPC Chat] No saved chat history found for '{agent.name}'.",
                        agent);
                }
            }

            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Use the button to delete the saved JSON conversation for this NPC. Works in edit mode too.",
                    MessageType.None);
            }
        }
    }
}
