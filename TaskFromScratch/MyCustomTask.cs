using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

namespace TaskFromScratch;

public class MyCustomTask
{
    private readonly Lock _lock = new();
    private bool _completed;
    private Exception? _exception;
    private Action? _action;
    private ExecutionContext? _context;

    public bool IsCompleted
    {
        get
        {
            lock (_lock)
            {
                return _completed;
            }
        }
    }

    public static MyCustomTask Delay(TimeSpan delay)
    {

        var task = new MyCustomTask();

        new Timer(_ => task.SetResult()).Change(delay, Timeout.InfiniteTimeSpan);

        return task;
    }

    public static MyCustomTask Run(Action action)
    {

        var task = new MyCustomTask();

        ThreadPool.QueueUserWorkItem(_ =>
        {
            try
            {
                action();
                task.SetResult();
            }
            catch (Exception e)
            {
                task.SetException(e);
            }
        });

        return task;
    }

    public MyCustomTask ContinueWith(Action action)
    {

        var task = new MyCustomTask();

        lock (_lock)
        {
            if (_completed)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        action();
                        task.SetResult();
                    }
                    catch (Exception e)
                    {
                        task.SetException(e);
                    }
                });
            }
            else
            {

                _action = action;
                _context = ExecutionContext.Capture();

            }

        }

        return task;
    }

    public MyCustomTaskAwaiter GetAwaiter() => new MyCustomTaskAwaiter(this);

    public void SetResult() => CompleteTask(null);

    public void SetException(Exception exception) => CompleteTask(exception);

    public void Wait()
    {
        ManualResetEventSlim resetEventSlim = null;

        lock (_lock)
        {
            if (!_completed)
            {
                resetEventSlim = new ManualResetEventSlim();
                ContinueWith(() => resetEventSlim.Set());


            }
        }

        resetEventSlim?.Wait();

        if (_exception is not null)
        {
            ExceptionDispatchInfo.Throw(_exception);
        }

    }

    private void CompleteTask(Exception? e)
    {
        lock (_lock)
        {
            if (_completed)
                throw new InvalidOperationException("Task already completed.");

            _completed = true;
            _exception = e;

            if (_action is not null)
            {

                if (_context is null)
                {
                    _action.Invoke();
                }
                else
                {

                    ExecutionContext.Run(_context, state => ((Action?)state)?.Invoke(), _action);
                }
            }
        }
    }
}

public readonly struct MyCustomTaskAwaiter : INotifyCompletion
{
    private readonly MyCustomTask _task;

    internal MyCustomTaskAwaiter(MyCustomTask task) => _task = task;

    public bool IsCompleted => _task.IsCompleted;

    public void OnCompleted(Action continuation) => _task.ContinueWith(continuation);

    public MyCustomTaskAwaiter GetAwait() => this;

    public void GetResult() => _task.Wait();
}
