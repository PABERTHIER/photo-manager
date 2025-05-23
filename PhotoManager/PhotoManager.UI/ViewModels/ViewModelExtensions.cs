﻿using System.Collections.ObjectModel;

namespace PhotoManager.UI.ViewModels;

public static class ViewModelExtensions
{
    public static void MoveUp<T>(this ObservableCollection<T> collection, T item)
    {
        int currentIndex = collection.IndexOf(item);

        if (currentIndex > 0)
        {
            int newIndex = currentIndex - 1;
            collection.Remove(item);
            collection.Insert(newIndex, item);
        }
    }

    public static void MoveDown<T>(this ObservableCollection<T> collection, T item)
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
