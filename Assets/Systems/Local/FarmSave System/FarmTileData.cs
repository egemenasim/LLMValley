using System;
using UnityEngine;
using LLMValley.Items;

[Serializable]
public class FarmTileData
{
    public int levelData;
    public int daysGrown;
    public ItemData plantItemData;
    public bool isTilled;
    public bool isWatered;
}
