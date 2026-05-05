using LLMValley.Items;

namespace LLMValley.Player
{
    public interface IToolAction
    {
        ItemType ItemType { get; }
        bool CanUse();
        void Use();
    }
}
