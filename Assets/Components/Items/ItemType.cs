namespace LLMValley.Items
{
    /// <summary>
    /// Categorises every item that can exist in LLM Valley.
    /// Used by <see cref="ItemData"/> to drive behaviour, UI icons, and shop logic.
    /// </summary>
    public enum ItemType
    {
        Seed, //Plant animation
        Crop, // Non animation
        Hoe, // Hoeing animation
        Fish, // Non animation
        Rod, // Fishing animation
        WaterCan, // Watering animation
        Currency, // No animation
        Misc // No animaion
    }
}
