using System.Globalization;

namespace PhotoManager.Tests.Unit.UI.Converters;

[TestFixture]
public class FileSizeConverterTests
{
    [Test]
    [TestCase(656, "656 bytes")]
    [TestCase(1024, "1.0 KB")]
    [TestCase(17734, "17.3 KB")]
    [TestCase(20480, "20.0 KB")]
    [TestCase(562688, "549.5 KB")]
    [TestCase(565248, "552.0 KB")]
    [TestCase(1048576, "1.0 MB")]
    [TestCase(54712102, "52.2 MB")]
    [TestCase(54956032, "52.4 MB")]
    [TestCase(1742919342, "1.6 GB")]
    [TestCase(1753653248, "1.6 GB")]
    [TestCase(24998490626, "23.3 GB")]
    [TestCase(25073561600, "23.4 GB")]
    public void Convert_FileWithDifferentSize_ReturnsFileSize(long size, string expected)
    {
        FileSizeConverter fileSizeConverter = new();
        object? parameter = null;

        string result = (string)fileSizeConverter.Convert(size, typeof(long), parameter!, CultureInfo.InvariantCulture);
        Assert.AreEqual(expected, result);
    }

    [Test]
    public void Convert_NullInput_Returns0Bytes()
    {
        FileSizeConverter fileSizeConverter = new();
        string? input = null;
        object? parameter = null;

        object result = fileSizeConverter.Convert(input!, typeof(long), parameter!, CultureInfo.InvariantCulture);

        Assert.AreEqual("0 bytes", result);
    }

    [Test]
    public void Convert_NonNumberInputType_ReturnsFileSize()
    {
        FileSizeConverter fileSizeConverter = new();
        string input = "12345";
        object? parameter = null;

        object result = fileSizeConverter.Convert(input, typeof(string), parameter!, CultureInfo.InvariantCulture);

        Assert.AreEqual("12.1 KB", result);
    }

    [Test]
    public void Convert_InvalidFormat_ThrowsFormatException()
    {
        FileSizeConverter fileSizeConverter = new();
        string input = "abc";
        object? parameter = null;

        Assert.Throws<FormatException>(() =>
        {
            fileSizeConverter.Convert(input, typeof(long), parameter!, CultureInfo.InvariantCulture);
        });
    }

    [Test]
    public void Convert_InvalidCast_ThrowsInvalidCastException()
    {
        FileSizeConverter fileSizeConverter = new();
        IConvertible input = new InvalidConvertible();
        object? parameter = null;

        Assert.Throws<InvalidCastException>(() =>
        {
            fileSizeConverter.Convert(input, typeof(long), parameter!, CultureInfo.InvariantCulture);
        });
    }

    [Test]
    public void Convert_Overflow_ThrowsOverflowException()
    {
        FileSizeConverter fileSizeConverter = new();
        string input = long.MaxValue.ToString() + "0"; // Adding a digit to exceed the maximum value
        object? parameter = null;

        Assert.Throws<OverflowException>(() =>
        {
            fileSizeConverter.Convert(input, typeof(long), parameter!, CultureInfo.InvariantCulture);
        });
    }

    [Test]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        FileSizeConverter fileSizeConverter = new();
        object? parameter = null;

        Assert.Throws<NotImplementedException>(() => fileSizeConverter.ConvertBack("17.3 KB", typeof(string), parameter!, CultureInfo.InvariantCulture));
    }

    private class InvalidConvertible : IConvertible
    {
        public TypeCode GetTypeCode() => throw new NotImplementedException();
        public bool ToBoolean(IFormatProvider? provider) => throw new NotImplementedException();
        public byte ToByte(IFormatProvider? provider) => throw new NotImplementedException();
        public char ToChar(IFormatProvider? provider) => throw new NotImplementedException();
        public DateTime ToDateTime(IFormatProvider? provider) => throw new NotImplementedException();
        public decimal ToDecimal(IFormatProvider? provider) => throw new NotImplementedException();
        public double ToDouble(IFormatProvider? provider) => throw new NotImplementedException();
        public short ToInt16(IFormatProvider? provider) => throw new NotImplementedException();
        public int ToInt32(IFormatProvider? provider) => throw new NotImplementedException();
        public long ToInt64(IFormatProvider? provider) => throw new InvalidCastException();
        public sbyte ToSByte(IFormatProvider? provider) => throw new NotImplementedException();
        public float ToSingle(IFormatProvider? provider) => throw new NotImplementedException();
        public string ToString(IFormatProvider? provider) => throw new NotImplementedException();
        public object ToType(Type conversionType, IFormatProvider? provider) => throw new NotImplementedException();
        public ushort ToUInt16(IFormatProvider? provider) => throw new NotImplementedException();
        public uint ToUInt32(IFormatProvider? provider) => throw new NotImplementedException();
        public ulong ToUInt64(IFormatProvider? provider) => throw new NotImplementedException();
    }
}
