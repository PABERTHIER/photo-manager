using Avalonia;
using Avalonia.Headless;
using Avalonia.Threading;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace PhotoManager.Tests;

public static class AvaloniaTestSetup
{
    private static readonly Lock SyncRoot = new();

    private static BlockingCollection<Action>? _workItems;
    private static Exception? _initializationException;
    private static bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        lock (SyncRoot)
        {
            if (_initialized)
            {
                return;
            }

            using (ManualResetEventSlim initializedEvent = new())
            {
                BlockingCollection<Action> workItems = [];
                _workItems = workItems;
                UiThreadState uiThreadState = new(initializedEvent, workItems);
                Thread uiThread = new(static state =>
                {
                    UiThreadState currentState = (UiThreadState)state!;
                    RunUiThread(currentState.InitializedEvent, currentState.WorkItems);
                })
                {
                    IsBackground = true,
                    Name = "Avalonia test UI thread"
                };
                uiThread.Start(uiThreadState);
                initializedEvent.Wait();

                if (_initializationException != null)
                {
                    ExceptionDispatchInfo.Capture(_initializationException).Throw();
                }

                _initialized = true;
            }
        }
    }

    public static Task RunOnUiThreadAsync(Action action)
    {
        EnsureInitialized();

        if (_workItems == null)
        {
            throw new InvalidOperationException("Avalonia test application has not been initialized.");
        }

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        _workItems.Add(() =>
        {
            try
            {
                action();
                taskCompletionSource.SetResult();
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }
        });

        return taskCompletionSource.Task;
    }

    public static Task RunOnUiThreadAsync(Func<Task> action)
    {
        EnsureInitialized();

        if (_workItems == null)
        {
            throw new InvalidOperationException("Avalonia test application has not been initialized.");
        }

        TaskCompletionSource taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        _workItems.Add(() =>
        {
            try
            {
                WaitForUiTask(action());
                taskCompletionSource.SetResult();
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }
        });

        return taskCompletionSource.Task;
    }

    public static Task<T> RunOnUiThreadAsync<T>(Func<T> action)
    {
        EnsureInitialized();

        if (_workItems == null)
        {
            throw new InvalidOperationException("Avalonia test application has not been initialized.");
        }

        TaskCompletionSource<T> taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        _workItems.Add(() =>
        {
            try
            {
                taskCompletionSource.SetResult(action());
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }
        });

        return taskCompletionSource.Task;
    }

    public static Task<T> RunOnUiThreadAsync<T>(Func<Task<T>> action)
    {
        EnsureInitialized();

        if (_workItems == null)
        {
            throw new InvalidOperationException("Avalonia test application has not been initialized.");
        }

        TaskCompletionSource<T> taskCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        _workItems.Add(() =>
        {
            try
            {
                Task<T> task = action();
                WaitForUiTask(task);
                taskCompletionSource.SetResult(task.GetAwaiter().GetResult());
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }
        });

        return taskCompletionSource.Task;
    }

    public static object? InvokeNonPublicInstanceMethod(object target, string methodName, params object?[] parameters)
    {
        MethodInfo methodInfo = target.GetType().GetMethod(
            methodName,
            BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingMethodException(target.GetType().FullName, methodName);

        return methodInfo.Invoke(target, parameters);
    }

    private static void WaitForUiTask(Task task)
    {
        while (!task.IsCompleted)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(1);
        }

        task.GetAwaiter().GetResult();
    }

    private static void RunUiThread(ManualResetEventSlim initializedEvent, BlockingCollection<Action> workItems)
    {
        try
        {
            InitializeAvalonia();
        }
        catch (Exception ex)
        {
            _initializationException = ex;
        }
        finally
        {
            initializedEvent.Set();
        }

        if (_initializationException != null)
        {
            return;
        }

        foreach (Action workItem in workItems.GetConsumingEnumerable())
        {
            workItem();
        }
    }

    private static void InitializeAvalonia()
    {
        AppBuilder.Configure<AvaloniaTestApplication>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
            .SetupWithoutStarting();
    }

    private sealed class UiThreadState(ManualResetEventSlim initializedEvent, BlockingCollection<Action> workItems)
    {
        public ManualResetEventSlim InitializedEvent { get; } = initializedEvent;
        public BlockingCollection<Action> WorkItems { get; } = workItems;
    }
}

internal sealed class AvaloniaTestApplication : Avalonia.Application;
