using UnityEditor;
using UnityEngine;

namespace Systems.Calendar.Editor
{
    [CustomEditor(typeof(CalendarEvent))]
    public class CalendarEventEditor : UnityEditor.Editor
    {
        private SerializedProperty eventName;
        private SerializedProperty description;
        private SerializedProperty icon;
        private SerializedProperty occurrenceType;
        private SerializedProperty triggerDay;
        private SerializedProperty triggerSeason;
        private SerializedProperty triggerYear;
        private SerializedProperty triggerDayOfWeek;
        private SerializedProperty repeatInterval;

        private void OnEnable()
        {
            eventName = serializedObject.FindProperty("eventName");
            description = serializedObject.FindProperty("description");
            icon = serializedObject.FindProperty("icon");
            occurrenceType = serializedObject.FindProperty("occurrenceType");
            triggerDay = serializedObject.FindProperty("triggerDay");
            triggerSeason = serializedObject.FindProperty("triggerSeason");
            triggerYear = serializedObject.FindProperty("triggerYear");
            triggerDayOfWeek = serializedObject.FindProperty("triggerDayOfWeek");
            repeatInterval = serializedObject.FindProperty("repeatInterval");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("General Info", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(eventName);
            EditorGUILayout.PropertyField(description);
            EditorGUILayout.PropertyField(icon);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Recurrence Pattern", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(occurrenceType);

            OccurrenceType type = (OccurrenceType)occurrenceType.enumValueIndex;

            EditorGUI.indentLevel++;

            switch (type)
            {
                case OccurrenceType.OneTime:
                    EditorGUILayout.PropertyField(triggerDay, new GUIContent("Day"));
                    EditorGUILayout.PropertyField(triggerSeason, new GUIContent("Season"));
                    EditorGUILayout.PropertyField(triggerYear, new GUIContent("Year"));
                    break;

                case OccurrenceType.Daily:
                    EditorGUILayout.PropertyField(repeatInterval, new GUIContent("Every X Days"));
                    break;

                case OccurrenceType.Weekly:
                    EditorGUILayout.PropertyField(triggerDayOfWeek, new GUIContent("Day of Week"));
                    EditorGUILayout.PropertyField(repeatInterval, new GUIContent("Every X Weeks"));
                    break;

                case OccurrenceType.Monthly:
                    EditorGUILayout.PropertyField(triggerDay, new GUIContent("Day of Month"));
                    EditorGUILayout.PropertyField(repeatInterval, new GUIContent("Every X Months"));
                    break;

                case OccurrenceType.Yearly:
                    EditorGUILayout.PropertyField(triggerDay, new GUIContent("Day"));
                    EditorGUILayout.PropertyField(triggerSeason, new GUIContent("Season"));
                    EditorGUILayout.PropertyField(repeatInterval, new GUIContent("Every X Years"));
                    break;
            }

            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
