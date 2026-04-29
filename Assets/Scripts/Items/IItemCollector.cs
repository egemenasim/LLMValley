using LLMValley.Items;

namespace LLMValley.Items
{
    /// <summary>
    /// Implement this interface on any component that can receive picked-up items.
    /// Attach it to the Player GameObject so <see cref="CollectibleItem"/> can
    /// hand items off without a hard dependency on the inventory system.
    /// </summary>
    public interface IItemCollector
    {
        /// <summary>
        /// Called by <see cref="CollectibleItem"/> when the player walks over it.
        /// </summary>
        /// <param name="item">The item definition.</param>
        /// <param name="quantity">Number of units collected.</param>
        void CollectItem(ItemData item, int quantity);
    }
}
