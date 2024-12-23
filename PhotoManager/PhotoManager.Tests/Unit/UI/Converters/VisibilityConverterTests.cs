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
        Assert.That(result, Is.EqualTo(Visibility.Hidden));
    }

    [Test]
    public void Convert_OtherType_ReturnsVisible()
    {
        VisibilityConverter visibilityConverter = new();
        object value = new SomeOtherType();
        object? parameter = null;

        object result = visibilityConverter.Convert(value, typeof(Visibility), parameter!, CultureInfo.InvariantCulture);

        Assert.That(result, Is.EqualTo(Visibility.Visible));
    }

    [Test]
    public void Convert_String_ReturnsVisible()
    {
        VisibilityConverter visibilityConverter = new();
        object? parameter = null;

        Visibility result = (Visibility)visibilityConverter.Convert("A string", typeof(Visibility), parameter!, CultureInfo.InvariantCulture);
        Assert.That(result, Is.EqualTo(Visibility.Visible));
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        VisibilityConverter visibilityConverter = new();
        object? parameter = null;

        NotImplementedException? exception = Assert.Throws<NotImplementedException>(() => visibilityConverter.ConvertBack(Visibility.Visible, typeof(Visibility), parameter!, CultureInfo.InvariantCulture));

        Assert.That(exception?.Message, Is.EqualTo("The method or operation is not implemented."));
    }

    private class NamedObject
    {
    }

    private class SomeOtherType
    {
    }
}
