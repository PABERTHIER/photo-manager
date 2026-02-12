using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace PhotoManager.UI.ViewModels;

public class SortableObservableCollection<T> : ObservableCollection<T>
{
    public void Sort(IComparer<T> comparer)
    {
        if (comparer == null)
        {
            throw new ArgumentNullException(nameof(comparer));
        }

        T[] sortedItems = new T[Count];
        Items.CopyTo(sortedItems, 0);

        sortedItems.AsSpan().Sort(comparer);

        for (int i = 0; i < sortedItems.Length; i++)
        {
            Items[i] = sortedItems[i];
        }

        // Raise a Reset notification so that UI bindings refresh.
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}
