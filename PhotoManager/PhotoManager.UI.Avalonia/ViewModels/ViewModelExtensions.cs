using System.Collections.ObjectModel;

namespace PhotoManager.UI.ViewModels;

public static class ViewModelExtensions
{
    extension<T>(ObservableCollection<T> collection)
    {
        public void MoveUp(T item)
        {
            int currentIndex = collection.IndexOf(item);

            if (currentIndex > 0)
            {
                int newIndex = currentIndex - 1;
                collection.Remove(item);
                collection.Insert(newIndex, item);
            }
        }

        public void MoveDown(T item)
        {
            int currentIndex = collection.IndexOf(item);

            if (currentIndex >= 0 && currentIndex < (collection.Count - 1))
            {
                int newIndex = currentIndex + 1;
                collection.Remove(item);
                collection.Insert(newIndex, item);
            }
        }
    }
}
