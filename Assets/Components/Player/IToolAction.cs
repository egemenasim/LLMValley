namespace LLMValley.Player
{
    public interface IToolAction
    {
        ToolType ToolType { get; }
        bool CanUse();
        void Use();
    }
}