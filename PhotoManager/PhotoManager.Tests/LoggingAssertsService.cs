using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;

namespace PhotoManager.Tests;

public class LoggingAssertsService
{
    private MemoryAppender? _memoryAppender;

    public LoggingAssertsService()
    {
        LoggingAssertSetUp();
    }

    private void LoggingAssertSetUp()
    {
        _memoryAppender = new();
        BasicConfigurator.Configure(_memoryAppender);
    }

    public void AssertLogExceptions (Exception[] expectedExceptions, Type typeOfService)
    {
        LoggingEvent[] loggingEvents = GetLoggingEvents();

        Assert.That(loggingEvents, Has.Length.EqualTo(expectedExceptions.Length));

        for (int i = 0; i < loggingEvents.Length; i++)
        {
            string? messageObject;

            if (loggingEvents[i].MessageObject is Exception exception)
            {
                messageObject = exception.Message;
            }
            else
            {
                messageObject = loggingEvents[i].MessageObject as string;
            }

            AssertMessage(expectedExceptions[i].Message, messageObject);

            Assert.That(loggingEvents[i].LoggerName, Is.EqualTo(typeOfService.ToString()));
        }
    }

    public void AssertLogInfos (string[] expectedMessages, Type typeOfService)
    {
        LoggingEvent[] loggingEvents = GetLoggingEvents();

        Assert.That(loggingEvents, Has.Length.EqualTo(expectedMessages.Length));

        for (int i = 0; i < loggingEvents.Length; i++)
        {
            string? messageObject = loggingEvents[i].MessageObject as string;
            AssertMessage(expectedMessages[i], messageObject);
            Assert.That(loggingEvents[i].LoggerName, Is.EqualTo(typeOfService.ToString()));
        }
    }

    public void LoggingAssertTearDown()
    {
        _memoryAppender = null;
        LogManager.Shutdown();
    }

    private LoggingEvent[] GetLoggingEvents()
    {
        LoggingEvent[] logMessages = _memoryAppender?.GetEvents() ?? [];

        return logMessages;
    }

    private static void AssertMessage(string expectedMessage, string? message)
    {
        Assert.That(message, Is.Not.Null);
        Assert.That(message, Is.EqualTo(expectedMessage));
    }
}
