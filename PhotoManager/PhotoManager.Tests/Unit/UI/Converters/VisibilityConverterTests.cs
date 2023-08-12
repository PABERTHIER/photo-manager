using NUnit.Framework;
using PhotoManager.UI.Converters;
using System.Globalization;
using System.Windows;

namespace PhotoManager.Tests.Unit.UI.Converters;

[TestFixture]
public class VisibilityConverterTests
{
    [Test]
    public void Convert_NamedObject_ReturnsHidden()
    {
        VisibilityConverter visibilityConverter = new();
        object? parameter = null;

        Visibility result = (Visibility)visibilityConverter.Convert(new NamedObject(), typeof(Visibility), parameter!, CultureInfo.InvariantCulture);
        Assert.AreEqual(Visibility.Hidden, result);
    }

    [Test]
    public void Convert_OtherType_ReturnsVisible()
    {
        VisibilityConverter visibilityConverter = new();
        object value = new SomeOtherType();
        object? parameter = null;

        var result = visibilityConverter.Convert(value, typeof(Visibility), parameter!, CultureInfo.InvariantCulture);

        Assert.AreEqual(Visibility.Visible, result);
    }

    [Test]
    public void Convert_String_ReturnsVisible()
    {
        VisibilityConverter visibilityConverter = new();
        object? parameter = null;

        Visibility result = (Visibility)visibilityConverter.Convert("A string", typeof(Visibility), parameter!, CultureInfo.InvariantCulture);
        Assert.AreEqual(Visibility.Visible, result);
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        VisibilityConverter visibilityConverter = new();
        object? parameter = null;

        Assert.Throws<NotImplementedException>(() => visibilityConverter.ConvertBack(Visibility.Visible, typeof(Visibility), parameter!, CultureInfo.InvariantCulture));
    }

    private class NamedObject
    {
    }

    private class SomeOtherType
    {
    }
}
