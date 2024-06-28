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

        Assert.AreEqual(expectedExceptions.Length, loggingEvents.Length);

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
            
            Assert.AreEqual(typeOfService.ToString(), loggingEvents[i].LoggerName);
        }
    }

    public void AssertLogInfos (string[] expectedMessages, Type typeOfService)
    {
        LoggingEvent[] loggingEvents = GetLoggingEvents();

        Assert.AreEqual(expectedMessages.Length, loggingEvents.Length);

        for (int i = 0; i < loggingEvents.Length; i++)
        {
            string? messageObject = loggingEvents[i].MessageObject as string;
            AssertMessage(expectedMessages[i], messageObject);
            Assert.AreEqual(typeOfService.ToString(), loggingEvents[i].LoggerName);
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
        Assert.IsNotNull(message);
        Assert.AreEqual(expectedMessage, message);
    }
}
