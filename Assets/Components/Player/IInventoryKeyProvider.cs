namespace LLMValley.Player
{
    public interface IInventoryKeyProvider
    {
        bool HasKey(string keyId);
        bool HasItemById(string itemId);
    }
}
