using System;

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

    public void SetResult() => CompleteTask(null);

    public void SetException(Exception exception) => CompleteTask(exception);

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
