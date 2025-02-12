using System.Collections.Specialized;

namespace PhotoManager.Tests.Unit.UI.ViewModels;

[TestFixture]
public class SortableObservableCollectionTests
{
    [Test]
    public void Constructor_CollectionIsEmpty_InitializesEmptyCollection()
    {
        SortableObservableCollection<int> collection = [];

        Assert.That(collection, Is.Empty);
    }

    [Test]
    public void Constructor_CollectionIsNotEmpty_CopiesItems()
    {
        List<int> items = [3, 1, 2];

        SortableObservableCollection<int> collection = [..items];

        Assert.That(collection, Is.EqualTo(items));
    }

    [Test]
    public void Sort_CollectionIsNotEmpty_SortsItemsInCollectionAndRaisesResetNotification()
    {
        SortableObservableCollection<int> collection = [3, 1, 2];

        bool eventRaised = false;
        collection.CollectionChanged += (_, args) =>
        {
            Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            eventRaised = true;
        };

        collection.Sort(Comparer<int>.Default);

        Assert.That(collection, Is.EqualTo([1, 2, 3 ]));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void Sort_CollectionIsEmpty_RaisesResetNotification()
    {
        SortableObservableCollection<int> collection = [];

        bool eventRaised = false;
        collection.CollectionChanged += (_, args) =>
        {
            Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            eventRaised = true;
        };

        collection.Sort(Comparer<int>.Default);

        Assert.That(collection, Is.Empty);
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void Sort_CollectionHasDuplicatedItems_SortsItemsInCollectionAndRaisesResetNotification()
    {
        SortableObservableCollection<int> collection = [5, 3, 3, 7, 1];

        bool eventRaised = false;
        collection.CollectionChanged += (_, args) =>
        {
            Assert.That(args.Action, Is.EqualTo(NotifyCollectionChangedAction.Reset));
            eventRaised = true;
        };

        collection.Sort(Comparer<int>.Default);

        Assert.That(collection, Is.EqualTo(new List<int> { 1, 3, 3, 5, 7 }));
        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void Sort_ComparerIsNull_ThrowsArgumentNullException()
    {
        SortableObservableCollection<int> collection = [3, 1, 2];

        bool eventRaised = false;
        collection.CollectionChanged += (_, _) =>
        {
            eventRaised = true;
        };

        Assert.That(() => collection.Sort(null!), Throws.ArgumentNullException);
        Assert.That(eventRaised, Is.False);
    }
}
