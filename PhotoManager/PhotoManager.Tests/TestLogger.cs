using Microsoft.Extensions.Logging;

namespace PhotoManager.Tests;

public class TestLogger<T> : ILogger<T>
{
    private readonly FakeLogger<T> _fakeLogger = new();

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
        => _fakeLogger.Log(logLevel, eventId, state, exception, formatter);

    public bool IsEnabled(LogLevel logLevel) => _fakeLogger.IsEnabled(logLevel);

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _fakeLogger.BeginScope(state);

    public void AssertLogExceptions(Exception[] expectedExceptions, Type typeOfService)
    {
        IReadOnlyList<FakeLogRecord> records = _fakeLogger.Collector.GetSnapshot();

        Assert.That(records, Has.Count.EqualTo(expectedExceptions.Length));

        for (int i = 0; i < records.Count; i++)
        {
            Assert.That(records[i].Level, Is.EqualTo(LogLevel.Error));
            string message = records[i].Exception?.Message ?? records[i].Message;
            AssertMessage(expectedExceptions[i].Message, message);
            Assert.That(records[i].Category, Is.EqualTo(typeOfService.ToString()));
        }
    }

    public void AssertLogErrors(string[] expectedMessages, Type typeOfService)
    {
        AssertLog(expectedMessages, typeOfService, LogLevel.Error);
    }

    public void AssertLogInfos(string[] expectedMessages, Type typeOfService)
    {
        AssertLog(expectedMessages, typeOfService, LogLevel.Information);
    }

    public void LoggingAssertTearDown() => _fakeLogger.Collector.Clear();

    private void AssertLog(string[] expectedMessages, Type typeOfService, LogLevel logLevel)
    {
        IReadOnlyList<FakeLogRecord> records = _fakeLogger.Collector.GetSnapshot();

        Assert.That(records, Has.Count.EqualTo(expectedMessages.Length));

        for (int i = 0; i < records.Count; i++)
        {
            Assert.That(records[i].Level, Is.EqualTo(logLevel));
            AssertMessage(expectedMessages[i], records[i].Message);
            Assert.That(records[i].Category, Is.EqualTo(typeOfService.ToString()));
        }
    }

    private static void AssertMessage(string expectedMessage, string? message)
    {
        Assert.That(message, Is.Not.Null);
        Assert.That(message, Is.EqualTo(expectedMessage));
    }
}
