using UnityEditor;
using UnityEngine;
using LLMValley.Items;

namespace LLMValley.Editor.Items
{
    /// <summary>
    /// Builds the CollectibleItem prefab programmatically so it is always
    /// in sync with the CollectibleItem component definition.
    ///
    /// Run via: Tools → LLMValley → Create CollectibleItem Prefab
    /// Re-running replaces the existing prefab.
    /// </summary>
    public static class CollectibleItemPrefabBuilder
    {
        private const string PrefabFolder = "Assets/Prefabs/Items";
        private const string PrefabPath   = PrefabFolder + "/CollectibleItem.prefab";

        [MenuItem("Tools/LLMValley/Create CollectibleItem Prefab")]
        public static void Build()
        {
            EnsureFolderExists(PrefabFolder);

            // ── Build the GameObject in memory ────────────────────────────────────
            GameObject go = new GameObject("CollectibleItem");

            // SpriteRenderer — no sprite assigned; CollectibleItem sets it at runtime
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Default";
            sr.sortingOrder     = 5;

            // CircleCollider2D — trigger so it doesn't block movement
            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius    = 0.3f;

            // Rigidbody2D — kinematic floater, no gravity, no rotation
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType     = RigidbodyType2D.Kinematic;
            rb.constraints  = RigidbodyConstraints2D.FreezeRotation;

            // CollectibleItem script
            CollectibleItem ci = go.AddComponent<CollectibleItem>();
            ci.quantity      = 1;
            ci.bobAmplitude  = 0.1f;
            ci.bobCycle      = 1f;

            // ── Save as prefab ────────────────────────────────────────────────────
            bool replacing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null;

            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(go, PrefabPath, out bool success);
            Object.DestroyImmediate(go); // clean up the scene-temporary object

            if (success)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                string action = replacing ? "Replaced" : "Created";
                Debug.Log($"[CollectibleItemPrefabBuilder] {action} prefab at: {PrefabPath}");

                EditorUtility.DisplayDialog(
                    "CollectibleItem Prefab Builder",
                    $"✅ {action} prefab successfully!\n\n{PrefabPath}",
                    "OK"
                );

                // Ping the new asset in the Project panel
                EditorGUIUtility.PingObject(prefabAsset);
            }
            else
            {
                Debug.LogError($"[CollectibleItemPrefabBuilder] Failed to save prefab at: {PrefabPath}");
                EditorUtility.DisplayDialog(
                    "CollectibleItem Prefab Builder",
                    $"❌ Failed to save prefab.\nCheck the Console for details.",
                    "OK"
                );
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static void EnsureFolderExists(string path)
        {
            string[] parts   = path.Split('/');
            string   current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
