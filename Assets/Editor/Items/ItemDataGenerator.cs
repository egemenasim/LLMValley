using UnityEditor;
using UnityEngine;
using LLMValley.Items;

namespace LLMValley.Editor.Items
{
    /// <summary>
    /// Unified editor utility that generates every ItemData asset for LLM Valley.
    /// Organised by category; each category writes to its own subfolder under
    /// Assets/Resources/Items/.
    ///
    /// Run via: Tools → LLMValley → Generate All Item Assets
    /// Safe to re-run — existing assets are skipped, not overwritten.
    /// </summary>
    public static class ItemDataGenerator
    {
        // ── Folder roots ──────────────────────────────────────────────────────────
        private const string RootFolder   = "Assets/Scriptables/Items/";
        private const string SeedsFolder  = RootFolder + "Seeds/";
        private const string CropsFolder  = RootFolder + "Crops/";
        private const string FishFolder   = RootFolder + "Fish/";
        private const string ToolsFolder  = RootFolder + "Tools/";

        // ─────────────────────────────────────────────────────────────────────────
        // Item definitions
        // (assetFileName, itemID, baseValue, displayName, description)
        // ─────────────────────────────────────────────────────────────────────────

        private static readonly ItemDef[] SeedDefs =
        {
            new("Wheat_Seed",      1001,  5f, "Wheat Seed",      "A simple wheat seed. Hardy and reliable."),
            new("Carrot_Seed",     1002,  8f, "Carrot Seed",     "A carrot seed. Grows into a crunchy orange root."),
            new("Potato_Seed",     1003, 10f, "Potato Seed",     "A potato seed. Hearty and filling when harvested."),
            new("Tomato_Seed",     1004, 12f, "Tomato Seed",     "A tomato seed. Produces a juicy, red fruit."),
            new("Corn_Seed",       1005, 15f, "Corn Seed",       "A corn seed. Tall stalks bearing golden ears."),
            new("Pumpkin_Seed",    1006, 20f, "Pumpkin Seed",    "A pumpkin seed. Grows a large, orange gourd."),
            new("Strawberry_Seed", 1007, 18f, "Strawberry Seed", "A strawberry seed. Sweet red berries loved by all."),
            new("Sunflower_Seed",  1008, 14f, "Sunflower Seed",  "A sunflower seed. Brightens the farm and attracts bees."),
            new("Cabbage_Seed",    1009,  9f, "Cabbage Seed",    "A cabbage seed. A sturdy leafy green for the garden."),
            new("Pepper_Seed",     1010, 16f, "Pepper Seed",     "A pepper seed. Adds a spicy kick to any dish."),
            new("Melon_Seed",      1011, 22f, "Melon Seed",      "A melon seed. Produces a large, sweet summer fruit."),
            new("Blueberry_Seed",  1012, 25f, "Blueberry Seed",  "A blueberry seed. Small but packed with flavour."),
        };

        private static readonly ItemDef[] CropDefs =
        {
            new("Wheat",      2001, 15f, "Wheat",      "Freshly harvested wheat. Used in baking and brewing."),
            new("Carrot",     2002, 25f, "Carrot",     "A crisp, orange carrot fresh from the soil."),
            new("Potato",     2003, 30f, "Potato",     "A starchy potato. Great roasted or in a stew."),
            new("Tomato",     2004, 35f, "Tomato",     "A ripe, juicy tomato bursting with flavour."),
            new("Corn",       2005, 40f, "Corn",       "A golden ear of corn, sweet and tender."),
            new("Pumpkin",    2006, 60f, "Pumpkin",    "A hefty pumpkin. Perfect for pies and lanterns."),
            new("Strawberry", 2007, 55f, "Strawberry", "A plump, red strawberry. Irresistibly sweet."),
            new("Sunflower",  2008, 45f, "Sunflower",  "A bright sunflower head full of seeds."),
            new("Cabbage",    2009, 28f, "Cabbage",    "A dense, leafy cabbage head."),
            new("Pepper",     2010, 48f, "Pepper",     "A fiery pepper that adds heat to any recipe."),
            new("Melon",      2011, 70f, "Melon",      "A giant, sweet melon — summer in a rind."),
            new("Blueberry",  2012, 80f, "Blueberry",  "A cluster of ripe blueberries, rich in antioxidants."),
        };

        private static readonly ItemDef[] FishDefs =
        {
            new("Common_Fish",    3001,   20f, "Common Fish",    "A common fish from the lake. Nothing special, but edible."),
            new("Uncommon_Fish",  3002,   50f, "Uncommon Fish",  "A slightly unusual fish with a pleasant flavour."),
            new("Rare_Fish",      3003,  120f, "Rare Fish",      "A rare catch — anglers dream of this one."),
            new("Epic_Fish",      3004,  300f, "Epic Fish",      "An extraordinary fish of legend-worthy size."),
            new("Legendary_Fish", 3005,  750f, "Legendary Fish", "A fish spoken of in tales around the campfire."),
            new("Mythic_Fish",    3006, 2000f, "Mythic Fish",    "Said to grant a wish to whoever catches it."),
        };

        private static readonly ItemDef[] ToolDefs =
        {
            new("Wood_Hoe",            4001,  50f, "Wooden Hoe",           "A basic wooden hoe for tilling soil."),
            new("Copper_Hoe",          4002, 150f, "Copper Hoe",           "A copper hoe — more durable than wood."),
            new("Iron_Hoe",            4003, 400f, "Iron Hoe",             "A sturdy iron hoe for seasoned farmers."),
            new("Wood_WateringCan",    4004,  50f, "Wooden Watering Can",  "A simple wooden can. Holds a small amount of water."),
            new("Copper_WateringCan",  4005, 150f, "Copper Watering Can",  "A copper watering can with a wider nozzle."),
            new("Iron_WateringCan",    4006, 400f, "Iron Watering Can",    "A large iron can — waters many crops at once."),
            new("Wood_FishingRod",     4007,  50f, "Wooden Fishing Rod",   "A makeshift rod whittled from a branch."),
            new("Copper_FishingRod",   4008, 150f, "Copper Fishing Rod",   "A copper-tipped rod with better sensitivity."),
            new("Iron_FishingRod",     4009, 400f, "Iron Fishing Rod",     "A precision-crafted iron rod for serious anglers."),
        };

        // ─────────────────────────────────────────────────────────────────────────

        [MenuItem("Tools/LLMValley/Generate All Item Assets")]
        public static void GenerateAll()
        {
            // Ensure every folder exists before writing assets
            EnsureFolderExists(SeedsFolder);
            EnsureFolderExists(CropsFolder);
            EnsureFolderExists(FishFolder);
            EnsureFolderExists(ToolsFolder);

            int created = 0;
            int skipped = 0;

            created += GenerateCategory(SeedDefs,  SeedsFolder, ItemType.Seed,  stackable: true,  ref skipped);
            created += GenerateCategory(CropDefs,  CropsFolder, ItemType.Crop,  stackable: true,  ref skipped);
            created += GenerateCategory(FishDefs,  FishFolder,  ItemType.Fish,  stackable: true,  ref skipped);
            created += GenerateCategory(ToolDefs,  ToolsFolder, ItemType.Tool,  stackable: false, ref skipped);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ItemDataGenerator] Done. Created: {created}, Skipped: {skipped}");

            EditorUtility.DisplayDialog(
                "Item Asset Generator",
                $"Generation complete!\n\n" +
                $"✅  Created : {created}\n" +
                $"⏭  Skipped : {skipped}\n\n" +
                $"Assets saved to:\n{RootFolder}/",
                "OK"
            );
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static int GenerateCategory(
            ItemDef[]  defs,
            string     folder,
            ItemType   type,
            bool       stackable,
            ref int    skipped)
        {
            int created = 0;

            foreach (var def in defs)
            {
                string assetPath = $"{folder}/{def.FileName}.asset";

                if (AssetDatabase.LoadAssetAtPath<ItemData>(assetPath) != null)
                {
                    Debug.Log($"[ItemDataGenerator] Skipped (already exists): {assetPath}");
                    skipped++;
                    continue;
                }

                ItemData asset = ScriptableObject.CreateInstance<ItemData>();
                asset.itemName    = def.DisplayName;
                asset.itemID      = def.ItemID;
                asset.itemType    = type;
                asset.baseValue   = def.BaseValue;
                asset.description = def.Description;
                asset.isStackable  = stackable;
                asset.maxStackSize = stackable ? 99 : 1;
                // icon intentionally left null — assign sprites per-asset later

                AssetDatabase.CreateAsset(asset, assetPath);
                Debug.Log($"[ItemDataGenerator] Created: {assetPath}");
                created++;
            }

            return created;
        }

        /// <summary>
        /// Creates every folder segment in <paramref name="path"/> that does not yet exist.
        /// AssetDatabase requires parent folders to exist before creating children.
        /// </summary>
        private static void EnsureFolderExists(string path)
        {
            string[] parts   = path.Split('/');
            string   current = parts[0]; // "Assets"

            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                    Debug.Log($"[ItemDataGenerator] Created folder: {next}");
                }
                current = next;
            }
        }

        // ── Data container ────────────────────────────────────────────────────────

        private readonly struct ItemDef
        {
            public readonly string FileName;
            public readonly int    ItemID;
            public readonly float  BaseValue;
            public readonly string DisplayName;
            public readonly string Description;

            public ItemDef(string fileName, int itemID, float baseValue,
                           string displayName, string description)
            {
                FileName    = fileName;
                ItemID      = itemID;
                BaseValue   = baseValue;
                DisplayName = displayName;
                Description = description;
            }
        }
    }
}
