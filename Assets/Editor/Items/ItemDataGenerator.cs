using UnityEditor;
using UnityEngine;
using LLMValley.Items;

namespace LLMValley.Editor.Items
{
    public static class ItemDataGenerator
    {
        private const string RootFolder   = "Assets/Scriptables/Items/";
        private const string SeedsFolder  = RootFolder + "Seeds/";
        private const string CropsFolder  = RootFolder + "Crops/";
        private const string FishFolder   = RootFolder + "Fish/";
        private const string ToolsFolder  = RootFolder + "Tools/";
        private const string HatsFolder   = RootFolder + "Hats/";
        private const string MiscFolder   = RootFolder + "Misc/";

        private const string IconsPath    = "Assets/Art/Objects/icons/";

        private static readonly ItemDef[] CropDefs =
        {
            new("apple",   2001, 20f, "Apple",   "A sweet and crunchy red apple."),
            new("corn",    2002, 15f, "Corn",    "A golden ear of corn, sweet and tender."),
            new("pumpkin", 2003, 50f, "Pumpkin", "A large, orange gourd perfect for pies."),
            new("tomato",  2004, 25f, "Tomato",  "A ripe, juicy tomato bursting with flavour."),
            new("wheat",   2005, 10f, "Wheat",   "Golden wheat, essential for baking."),
        };

        private static readonly ItemDef[] FishDefs =
        {
            new("goldfish",   3002, 100f, "Goldfish",    "A beautiful, shimmering goldfish."),
            new("grayfish",   3003,  45f, "Grayfish",    "A sturdy gray fish, quite common."),
            new("silverfish", 3004,  75f, "Silverfish",  "A quick silver fish with reflective scales."),
        };

        private static readonly ItemDef[] ToolDefs =
        {
            new("hoe",   4001, 50f, "Hoe",          "A basic tool for tilling soil."),
            new("water", 4004, 50f, "Watering Can", "Essential for keeping your crops hydrated."),
            new("Rod",   4007, 50f, "Fishing Rod",  "A simple rod for catching fish."),
        };

        private static readonly ItemDef[] HatDefs =
        {
            new("beanie",  5001, 120f, "Beanie",      "A warm and stylish woollen beanie."),
            new("blue",    5002, 150f, "Blue Hat",    "A fashionable blue hat."),
            new("cowboy",  5003, 200f, "Cowboy Hat",  "A classic wide-brimmed cowboy hat."),
            new("english", 5004, 250f, "English Hat", "A sophisticated English-style hat."),
            new("fisher",  5005, 180f, "Fisher Hat",  "A practical hat for long days by the water."),
            new("straw",   5006,  80f, "Straw Hat",   "A light straw hat, perfect for farming."),
        };

        private static readonly ItemDef[] MiscDefs =
        {
            new("scarecrow", 6001, 300f, "Scarecrow", "Keeps pesky birds away from your crops."),
            new("sprinkler", 6002, 500f, "Sprinkler", "Automatically waters nearby tiles."),
            new("wood",      6003,   2f, "Wood",      "A basic resource for crafting and building."),
        };

        [MenuItem("Tools/LLMValley/Generate All Item Assets")]
        public static void GenerateAll()
        {
            if (AssetDatabase.IsValidFolder(RootFolder))
            {
                AssetDatabase.DeleteAsset(RootFolder);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EnsureFolderExists(SeedsFolder);
            EnsureFolderExists(CropsFolder);
            EnsureFolderExists(FishFolder);
            EnsureFolderExists(ToolsFolder);
            EnsureFolderExists(HatsFolder);
            EnsureFolderExists(MiscFolder);

            int created = 0;
            int skipped = 0;

            ItemDef[] seedDefs = new ItemDef[CropDefs.Length];
            for (int i = 0; i < CropDefs.Length; i++)
            {
                var crop = CropDefs[i];
                seedDefs[i] = new ItemDef(crop.FileName + "_Seed", crop.ItemID - 1000, crop.BaseValue / 2f, crop.DisplayName + " Seed", $"Plant this to grow {crop.DisplayName}.");
            }

            created += GenerateCategory(seedDefs, SeedsFolder, ItemType.Seed, true, ref skipped);
            created += GenerateCategory(CropDefs, CropsFolder, ItemType.Crop, true, ref skipped);
            created += GenerateCategory(FishDefs, FishFolder, ItemType.Fish, true, ref skipped);
            created += GenerateCategory(ToolDefs, ToolsFolder, ItemType.Misc, false, ref skipped, true);
            created += GenerateCategory(HatDefs, HatsFolder, ItemType.Misc, false, ref skipped);
            created += GenerateCategory(MiscDefs, MiscFolder, ItemType.Misc, true, ref skipped);

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


        private static int GenerateCategory(ItemDef[] defs, string folder, ItemType defaultType, bool stackable, ref int skipped, bool forceType = false)
        {
            int created = 0;
            foreach (var def in defs)
            {
                string assetPath = $"{folder}/{def.FileName}.asset";
                if (AssetDatabase.LoadAssetAtPath<ItemData>(assetPath) != null)
                {
                    skipped++;
                    continue;
                }

                ItemData asset = ScriptableObject.CreateInstance<ItemData>();
                asset.itemName = def.DisplayName;
                asset.itemID = def.ItemID;
                asset.description = def.Description;
                asset.isStackable = stackable;
                asset.maxStackSize = stackable ? 99 : 1;
                asset.baseValue = def.BaseValue;

                if (forceType)
                {
                    if (def.FileName.ToLower().Contains("hoe")) asset.itemType = ItemType.Hoe;
                    else if (def.FileName.ToLower().Contains("water")) asset.itemType = ItemType.WaterCan;
                    else if (def.FileName.ToLower().Contains("rod")) asset.itemType = ItemType.Rod;
                    else asset.itemType = ItemType.Misc;
                }
                else
                {
                    asset.itemType = defaultType;
                }

                string iconName = def.FileName.Replace("_Seed", "").ToLower();
                string iconPath = $"{IconsPath}{iconName}.png";
                asset.icon = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);

                AssetDatabase.CreateAsset(asset, assetPath);
                created++;
            }
            return created;
        }

        private static void EnsureFolderExists(string path)
        {
            string[] parts = path.Split(new[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private readonly struct ItemDef
        {
            public readonly string FileName;
            public readonly int ItemID;
            public readonly float BaseValue;
            public readonly string DisplayName;
            public readonly string Description;

            public ItemDef(string fileName, int itemID, float baseValue, string displayName, string description)
            {
                FileName = fileName;
                ItemID = itemID;
                BaseValue = baseValue;
                DisplayName = displayName;
                Description = description;
            }
        }
    }
}
