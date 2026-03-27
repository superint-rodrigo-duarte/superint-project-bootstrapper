using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace superint.ProjectBootstrapper.UI.Collections;

/// <summary>
/// An ObservableCollection that automatically removes the oldest items when the limit is exceeded.
/// Useful for log management and other scenarios where memory should be bounded.
/// </summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
public class LimitedObservableCollection<T> : ObservableCollection<T>
{
    private readonly int _maxSize;

    /// <summary>
    /// Creates a new LimitedObservableCollection with the specified maximum size.
    /// </summary>
    /// <param name="maxSize">Maximum number of items to keep in the collection.</param>
    public LimitedObservableCollection(int maxSize = 500)
    {
        if (maxSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSize), "Max size must be greater than zero.");

        _maxSize = maxSize;
    }

    /// <summary>
    /// Gets the maximum size of the collection.
    /// </summary>
    public int MaxSize => _maxSize;

    /// <summary>
    /// Adds an item to the collection, removing the oldest item if the limit is exceeded.
    /// </summary>
    public new void Add(T item)
    {
        if (Count >= _maxSize)
        {
            RemoveAt(0);
        }

        base.Add(item);
    }

    /// <summary>
    /// Adds multiple items to the collection, removing oldest items as needed.
    /// </summary>
    public void AddRange(IEnumerable<T> items)
    {
        var itemList = items.ToList();

        // If adding more items than max size, only keep the last maxSize items
        if (itemList.Count >= _maxSize)
        {
            Clear();
            foreach (var item in itemList.TakeLast(_maxSize))
            {
                base.Add(item);
            }
            return;
        }

        // Remove items to make room
        var itemsToRemove = Math.Max(0, Count + itemList.Count - _maxSize);
        for (var i = 0; i < itemsToRemove; i++)
        {
            RemoveAt(0);
        }

        foreach (var item in itemList)
        {
            base.Add(item);
        }
    }

    /// <summary>
    /// Inserts an item at the specified index, removing the oldest item if the limit is exceeded.
    /// </summary>
    protected override void InsertItem(int index, T item)
    {
        if (Count >= _maxSize && index == Count)
        {
            // When adding at the end and at capacity, remove the first item
            RemoveAt(0);
            index = Math.Max(0, index - 1);
        }
        else if (Count >= _maxSize)
        {
            // When inserting in the middle at capacity, remove the last item
            RemoveAt(Count - 1);
        }

        base.InsertItem(index, item);
    }
}
