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

    public void AssertLogs (Exception[] expectedExceptions, Type typeOfService)
    {
        LoggingEvent[] logMessages = GetLoggingEvents();

        Assert.AreEqual(expectedExceptions.Length, logMessages.Length);

        for (int i = 0; i < logMessages.Length; i++)
        {
            Exception? logMessage = logMessages[i].MessageObject as Exception;
            Assert.IsNotNull(logMessage);
            Assert.AreEqual(expectedExceptions[i].GetType(), logMessage!.GetType());
            Assert.AreEqual(expectedExceptions[i].Message, logMessage.Message);
            Assert.AreEqual(typeOfService.ToString(), logMessages[i].LoggerName);
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
}
