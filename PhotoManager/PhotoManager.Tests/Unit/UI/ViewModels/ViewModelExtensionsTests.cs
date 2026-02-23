using System.Collections.ObjectModel;

namespace PhotoManager.Tests.Unit.UI.ViewModels;

[TestFixture]
public class ViewModelExtensionsTests
{
    [Test]
    public void MoveUp_FirstItem_DoesNotMoveUpItem()
    {
        ObservableCollection<string> items = ["toto", "tutu", "tata"];

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo("toto"));
        Assert.That(items[1], Is.EqualTo("tutu"));
        Assert.That(items[2], Is.EqualTo("tata"));

        items.MoveUp(items[0]);

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo("toto"));
        Assert.That(items[1], Is.EqualTo("tutu"));
        Assert.That(items[2], Is.EqualTo("tata"));
    }

    [Test]
    public void MoveUp_NotFirstAndNotLastItem_MovesUpItem()
    {
        ObservableCollection<string> items = ["toto", "tutu", "tata"];

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo("toto"));
        Assert.That(items[1], Is.EqualTo("tutu"));
        Assert.That(items[2], Is.EqualTo("tata"));

        items.MoveUp(items[1]);

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo("tutu"));
        Assert.That(items[1], Is.EqualTo("toto"));
        Assert.That(items[2], Is.EqualTo("tata"));
    }

    [Test]
    public void MoveUp_LastAndNotFirstItem_MovesUpItem()
    {
        ObservableCollection<string> items = ["toto", "tutu", "tata"];

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo("toto"));
        Assert.That(items[1], Is.EqualTo("tutu"));
        Assert.That(items[2], Is.EqualTo("tata"));

        items.MoveUp(items[2]);

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo("toto"));
        Assert.That(items[1], Is.EqualTo("tata"));
        Assert.That(items[2], Is.EqualTo("tutu"));
    }

    [Test]
    [TestCase(new[] { "toto", "toto", "toto" }, new[] { "toto", "toto", "toto" })] // Unchanged
    [TestCase(new[] { "tutu", "toto", "toto" }, new[] { "tutu", "toto", "toto" })] // Unchanged
    [TestCase(new[] { "toto", "tutu", "toto" }, new[] { "toto", "tutu", "toto" })] // Unchanged
    [TestCase(new[] { "toto", "toto", "tutu" }, new[] { "toto", "toto", "tutu" })] // Unchanged
    public void MoveUp_FirstItemAndDuplicateItemsInTheCollection_DoesNotMoveUpItem(string[] collection,
        string[] expectedCollection)
    {
        ObservableCollection<string> items = [.. collection];

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo(collection[0]));
        Assert.That(items[1], Is.EqualTo(collection[1]));
        Assert.That(items[2], Is.EqualTo(collection[2]));

        items.MoveUp(items[0]);

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo(expectedCollection[0]));
        Assert.That(items[1], Is.EqualTo(expectedCollection[1]));
        Assert.That(items[2], Is.EqualTo(expectedCollection[2]));
    }

    [Test]
    [TestCase(new[] { "toto", "toto", "toto" }, new[] { "toto", "toto", "toto" })] // Unchanged
    [TestCase(new[] { "tutu", "toto", "toto" }, new[] { "toto", "tutu", "toto" })]
    [TestCase(new[] { "toto", "tutu", "toto" }, new[] { "tutu", "toto", "toto" })]
    [TestCase(new[] { "toto", "toto", "tutu" }, new[] { "toto", "toto", "tutu" })] // Unchanged
    public void MoveUp_NotFirstAndNotLastItemAndDuplicateItemsInTheCollection_MovesUpFirstItemDuplicate(
        string[] collection, string[] expectedCollection)
    {
        ObservableCollection<string> items = [.. collection];

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo(collection[0]));
        Assert.That(items[1], Is.EqualTo(collection[1]));
        Assert.That(items[2], Is.EqualTo(collection[2]));

        items.MoveUp(items[1]);

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo(expectedCollection[0]));
        Assert.That(items[1], Is.EqualTo(expectedCollection[1]));
        Assert.That(items[2], Is.EqualTo(expectedCollection[2]));
    }

    [Test]
    [TestCase(new[] { "toto", "toto", "toto" }, new[] { "toto", "toto", "toto" })] // Unchanged
    [TestCase(new[] { "tutu", "toto", "toto" }, new[] { "toto", "tutu", "toto" })]
    [TestCase(new[] { "toto", "tutu", "toto" }, new[] { "toto", "tutu", "toto" })] // Unchanged
    [TestCase(new[] { "toto", "toto", "tutu" }, new[] { "toto", "tutu", "toto" })]
    public void MoveUp_LastItemAndDuplicateItemsInTheCollection_MovesUpFirstItemDuplicate(string[] collection,
        string[] expectedCollection)
    {
        ObservableCollection<string> items = [.. collection];

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo(collection[0]));
        Assert.That(items[1], Is.EqualTo(collection[1]));
        Assert.That(items[2], Is.EqualTo(collection[2]));

        items.MoveUp(items[2]);

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo(expectedCollection[0]));
        Assert.That(items[1], Is.EqualTo(expectedCollection[1]));
        Assert.That(items[2], Is.EqualTo(expectedCollection[2]));
    }

    [Test]
    public void MoveUp_OnlyOneItem_DoesNothing()
    {
        ObservableCollection<string> items = ["toto"];

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0], Is.EqualTo("toto"));

        items.MoveUp(items[0]);

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0], Is.EqualTo("toto"));
    }

    [Test]
    public void MoveUp_ItemIsNotFound_DoesNothing()
    {
        ObservableCollection<string> items = ["toto"];

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0], Is.EqualTo("toto"));

        items.MoveUp("tutu");

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0], Is.EqualTo("toto"));
    }

    [Test]
    public void MoveUp_NoItem_DoesNothing()
    {
        ObservableCollection<string> items = [];

        Assert.That(items, Is.Empty);

        items.MoveUp("toto");

        Assert.That(items, Is.Empty);
    }

    [Test]
    public void MoveUp_CollectionIsNull_ThrowsNullReferenceException()
    {
        ObservableCollection<string> items = null!;

        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => items.MoveUp("toto"));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
    }

    [Test]
    public void MoveUp_ItemIsNull_DoesNothing()
    {
        ObservableCollection<string> items = ["toto", "tutu", "tata"];

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo("toto"));
        Assert.That(items[1], Is.EqualTo("tutu"));
        Assert.That(items[2], Is.EqualTo("tata"));

        items.MoveUp(null!);

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo("toto"));
        Assert.That(items[1], Is.EqualTo("tutu"));
        Assert.That(items[2], Is.EqualTo("tata"));
    }

    [Test]
    public void MoveDown_FirstAndNotLastItem_MovesDownItem()
    {
        ObservableCollection<string> items = ["toto", "tutu", "tata"];

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo("toto"));
        Assert.That(items[1], Is.EqualTo("tutu"));
        Assert.That(items[2], Is.EqualTo("tata"));

        items.MoveDown(items[0]);

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo("tutu"));
        Assert.That(items[1], Is.EqualTo("toto"));
        Assert.That(items[2], Is.EqualTo("tata"));
    }

    [Test]
    public void MoveDown_NotFirstAndNotLastItem_MovesDownItem()
    {
        ObservableCollection<string> items = ["toto", "tutu", "tata"];

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo("toto"));
        Assert.That(items[1], Is.EqualTo("tutu"));
        Assert.That(items[2], Is.EqualTo("tata"));

        items.MoveDown(items[1]);

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo("toto"));
        Assert.That(items[1], Is.EqualTo("tata"));
        Assert.That(items[2], Is.EqualTo("tutu"));
    }

    [Test]
    public void MoveDown_LastItem_DoesNotMoveDownItem()
    {
        ObservableCollection<string> items = ["toto", "tutu", "tata"];

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo("toto"));
        Assert.That(items[1], Is.EqualTo("tutu"));
        Assert.That(items[2], Is.EqualTo("tata"));

        items.MoveDown(items[2]);

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo("toto"));
        Assert.That(items[1], Is.EqualTo("tutu"));
        Assert.That(items[2], Is.EqualTo("tata"));
    }

    [Test]
    [TestCase(new[] { "toto", "toto", "toto" }, new[] { "toto", "toto", "toto" })] // Unchanged
    [TestCase(new[] { "tutu", "toto", "toto" }, new[] { "toto", "tutu", "toto" })]
    [TestCase(new[] { "toto", "tutu", "toto" }, new[] { "tutu", "toto", "toto" })]
    [TestCase(new[] { "toto", "toto", "tutu" }, new[] { "toto", "toto", "tutu" })] // Unchanged
    public void MoveDown_FirstItemAndDuplicateItemsInTheCollection_DoesNotMoveDownItem(string[] collection,
        string[] expectedCollection)
    {
        ObservableCollection<string> items = [.. collection];

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo(collection[0]));
        Assert.That(items[1], Is.EqualTo(collection[1]));
        Assert.That(items[2], Is.EqualTo(collection[2]));

        items.MoveDown(items[0]);

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo(expectedCollection[0]));
        Assert.That(items[1], Is.EqualTo(expectedCollection[1]));
        Assert.That(items[2], Is.EqualTo(expectedCollection[2]));
    }

    [Test]
    [TestCase(new[] { "toto", "toto", "toto" }, new[] { "toto", "toto", "toto" })] // Unchanged
    [TestCase(new[] { "tutu", "toto", "toto" }, new[] { "tutu", "toto", "toto" })] // Unchanged
    [TestCase(new[] { "toto", "tutu", "toto" }, new[] { "toto", "toto", "tutu" })]
    [TestCase(new[] { "toto", "toto", "tutu" }, new[] { "toto", "toto", "tutu" })] // Unchanged
    public void MoveDown_NotFirstAndNotLastItemAndDuplicateItemsInTheCollection_MovesDownFirstItemDuplicate(
        string[] collection, string[] expectedCollection)
    {
        ObservableCollection<string> items = [.. collection];

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo(collection[0]));
        Assert.That(items[1], Is.EqualTo(collection[1]));
        Assert.That(items[2], Is.EqualTo(collection[2]));

        items.MoveDown(items[1]);

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo(expectedCollection[0]));
        Assert.That(items[1], Is.EqualTo(expectedCollection[1]));
        Assert.That(items[2], Is.EqualTo(expectedCollection[2]));
    }

    [Test]
    [TestCase(new[] { "toto", "toto", "toto" }, new[] { "toto", "toto", "toto" })] // Unchanged
    [TestCase(new[] { "tutu", "toto", "toto" }, new[] { "tutu", "toto", "toto" })] // Unchanged
    [TestCase(new[] { "toto", "tutu", "toto" }, new[] { "tutu", "toto", "toto" })]
    [TestCase(new[] { "toto", "toto", "tutu" }, new[] { "toto", "toto", "tutu" })] // Unchanged
    public void MoveDown_LastItemAndDuplicateItemsInTheCollection_MovesDownFirstItemDuplicate(string[] collection,
        string[] expectedCollection)
    {
        ObservableCollection<string> items = [.. collection];

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo(collection[0]));
        Assert.That(items[1], Is.EqualTo(collection[1]));
        Assert.That(items[2], Is.EqualTo(collection[2]));

        items.MoveDown(items[2]);

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo(expectedCollection[0]));
        Assert.That(items[1], Is.EqualTo(expectedCollection[1]));
        Assert.That(items[2], Is.EqualTo(expectedCollection[2]));
    }

    [Test]
    public void MoveDown_OnlyOneItem_DoesNothing()
    {
        ObservableCollection<string> items = ["toto"];

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0], Is.EqualTo("toto"));

        items.MoveDown(items[0]);

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0], Is.EqualTo("toto"));
    }

    [Test]
    public void MoveDown_ItemIsNotFound_DoesNothing()
    {
        ObservableCollection<string> items = ["toto"];

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0], Is.EqualTo("toto"));

        items.MoveDown("tutu");

        Assert.That(items, Has.Count.EqualTo(1));
        Assert.That(items[0], Is.EqualTo("toto"));
    }

    [Test]
    public void MoveDown_NoItem_DoesNothing()
    {
        ObservableCollection<string> items = [];

        Assert.That(items, Is.Empty);

        items.MoveDown("toto");

        Assert.That(items, Is.Empty);
    }

    [Test]
    public void MoveDown_CollectionIsNull_ThrowsNullReferenceException()
    {
        ObservableCollection<string> items = null!;

        NullReferenceException? exception = Assert.Throws<NullReferenceException>(() => items.MoveDown("toto"));

        Assert.That(exception?.Message, Is.EqualTo("Object reference not set to an instance of an object."));
    }

    [Test]
    public void MoveDown_ItemIsNull_DoesNothing()
    {
        ObservableCollection<string> items = ["toto", "tutu", "tata"];

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo("toto"));
        Assert.That(items[1], Is.EqualTo("tutu"));
        Assert.That(items[2], Is.EqualTo("tata"));

        items.MoveDown(null!);

        Assert.That(items, Has.Count.EqualTo(3));
        Assert.That(items[0], Is.EqualTo("toto"));
        Assert.That(items[1], Is.EqualTo("tutu"));
        Assert.That(items[2], Is.EqualTo("tata"));
    }
}
