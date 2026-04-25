namespace LLMValley.Items
{
    /// <summary>
    /// Categorises every item that can exist in LLM Valley.
    /// Used by <see cref="ItemData"/> to drive behaviour, UI icons, and shop logic.
    /// </summary>
    public enum ItemType
    {
        Seed,
        Crop,
        Fish,
        Tool,
        Currency,
        Misc
    }
}
