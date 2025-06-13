using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

// AsyncLocal<int> CaptureI = new();
// List<MyTask> tasks = new();
// for (int i = 0; i < 100; i++)
// {
//     CaptureI.Value = i;
//     tasks.Add(MyTask.Run(delegate { Console.Write(CaptureI.Value); }));
// }
//
// MyTask.WhenAll(tasks).Wait();

MyTask.Run(delegate { Console.Write("Hello"); }
    )
    .ContinueWith(delegate
        {
            Console.WriteLine(" World");
            return MyTask.Delay(1000).ContinueWith(delegate { Console.WriteLine(" Again"); });
        }
    ).Wait();

static async MyTask PrintAsync()
{
    for (int i = 0; i < 10; i++)
    {
        await MyTask.Delay(1000);
        Console.WriteLine(i);
    }
}

PrintAsync().Wait();

public class MyTaskMethodBuilder
{
    // The "Promise" the builder completes:
    private MyTask _task = new MyTask();

    // 1) Factory the compiler uses to get a builder
    public static MyTaskMethodBuilder Create() => new MyTaskMethodBuilder();

    // 2) What the compiler will expose as the return value of the async method
    public MyTask Task => _task;

    // 3) Called when your async method returns successfully
    public void SetResult() => _task.SetResult();

    // 4) Called when your async method throws
    public void SetException(Exception e) => _task.SetException(e);

    // 5) Suspend the state machine when you hit an 'await'
    public void AwaitOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter,
        ref TStateMachine stateMachine)
        where TAwaiter : INotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        // tell the awaiter to resume the state machine
        awaiter.OnCompleted(stateMachine.MoveNext);
    }

    // 6) Same as above but for `await`-in-`catch`/`finally`
    public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(
        ref TAwaiter awaiter,
        ref TStateMachine stateMachine)
        where TAwaiter : ICriticalNotifyCompletion
        where TStateMachine : IAsyncStateMachine
    {
        awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
    }

    // 7) Entry point: called once, at the start of the async method
    public void Start<TStateMachine>(ref TStateMachine stateMachine)
        where TStateMachine : IAsyncStateMachine
    {
        stateMachine.MoveNext();
    }

    // 8) Used for async `void` methods—not relevant here
    public void SetStateMachine(IAsyncStateMachine stateMachine) { }
}


[AsyncMethodBuilder(typeof(MyTaskMethodBuilder))]
public class MyTask
{
    private bool _completed;
    private Exception? _exception;
    private Action? _onComplete;
    private ExecutionContext? _executionContext;

    public struct Awaiter(MyTask t) : INotifyCompletion
    {
        public Awaiter GetAwaiter() => this;

        public bool IsCompleted => t.IsCompleted;
        
        public void OnCompleted(Action continuation) => t.ContinueWith(continuation);

        public void GetResult() => t.Wait();
    }
    
    public Awaiter GetAwaiter() => new(this); 
    
    public bool IsCompleted
    {
        get
        {
            lock (this)
            {
                return _completed;
            }
        }
    }

    public void SetResult() => Complete(null);

    public void SetException(Exception e) => Complete(e);

    private void Complete(Exception? e)
    {
        lock (this)
        {
            if (_completed)
            {
                throw new Exception("Bruh");
            }

            _exception = e;
            _completed = true;

            if (_onComplete is not null)
            {
                MyThreadPool.QueueUserWorkItem(delegate
                {
                    if (_executionContext is null)
                    {
                        _onComplete();
                    }
                    else
                    {
                        ExecutionContext.Run(
                            _executionContext,
                            state => ((Action)state!).Invoke(),
                            _onComplete);
                    }
                }, false);
            }
        }
    }

    public void Wait()
    {
        ManualResetEventSlim? mre = null;

        lock (this)
        {
            if (!_completed)
            {
                mre = new ManualResetEventSlim();
                ContinueWith(mre.Set);
            }
        }

        mre?.Wait();

        if (_exception is not null)
        {
            ExceptionDispatchInfo.Throw(_exception);
        }
    }

    public MyTask ContinueWith(Action action)
    {
        MyTask t = new();

        Action onComplete = () =>
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                t.SetException(e);
                return;
            }

            t.SetResult();
        };

        lock (this)
        {
            if (_completed)
            {
                MyThreadPool.QueueUserWorkItem(onComplete);
            }
            else
            {
                _onComplete = onComplete;
                _executionContext = ExecutionContext.Capture();
            }
        }

        return t;
    }

    public MyTask ContinueWith(Func<MyTask> action)
    {
        MyTask t = new();

        Action onComplete = () =>
        {
            try
            {
                MyTask next = action();
                next.ContinueWith(delegate
                {
                    if (next._exception is not null)
                    {
                        t.SetException(next._exception);
                    }
                    else
                    {
                        t.SetResult();
                    }
                });
            }
            catch (Exception e)
            {
                t.SetException(e);
                return;
            }
        };

        lock (this)
        {
            if (_completed)
            {
                MyThreadPool.QueueUserWorkItem(onComplete);
            }
            else
            {
                _onComplete = onComplete;
                _executionContext = ExecutionContext.Capture();
            }
        }

        return t;
    }

    public static MyTask Run(Action action)
    {
        MyTask t = new();

        MyThreadPool.QueueUserWorkItem(() =>
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                t.SetException(e);
            }

            t.SetResult();
        });

        return t;
    }

    public static MyTask WhenAll(List<MyTask> tasks)
    {
        MyTask t = new();

        if (tasks.Count == 0)
        {
            t.SetResult();
        }
        else
        {
            int remaining = tasks.Count;

            Action onComplete = () =>
            {
                if (Interlocked.Decrement(ref remaining) == 0)
                {
                    // Extract exceptions from all tasks objects and aggregate them in one and set it to current task
                    t.SetResult();
                }
            };

            foreach (var task in tasks)
            {
                t.ContinueWith(onComplete);
            }
        }

        return t;
    }

    public static MyTask Delay(int timeout)
    {
        MyTask t = new();

        new Timer(_ => t.SetResult()).Change(timeout, 0);

        return t;
    }

    public static MyTask Iterate(IEnumerable<MyTask> tasks)
    {
        MyTask t = new();

        IEnumerator<MyTask> enumerator = tasks.GetEnumerator();

        void MoveNext()
        {
            try
            {
                if (enumerator.MoveNext())
                {
                    MyTask next = enumerator.Current;
                    next.ContinueWith(MoveNext);
                    return;
                }
            }
            catch (Exception e)
            {
                t.SetException(e);
                return;
            }

            t.SetResult();
        }

        MoveNext();

        return t;
    }
}

static class MyThreadPool
{
    public static readonly BlockingCollection<(Action, ExecutionContext?)> _workItems = new();


    public static void QueueUserWorkItem(Action action, bool captureContext = true)
    {
        _workItems.Add((action, captureContext ? ExecutionContext.Capture() : null));
    }

    static MyThreadPool()
    {
        for (var i = 0; i < Environment.ProcessorCount; i++)
        {
            new Thread(() =>
            {
                while (true)
                {
                    (Action workItem, ExecutionContext? executionContext) = _workItems.Take();

                    if (executionContext is null)
                    {
                        workItem();
                    }
                    else
                    {
                        ExecutionContext.Run(
                            executionContext,
                            state => ((Action)state!).Invoke(),
                            workItem);
                    }
                }
            })
            {
                IsBackground = true
            }.Start();
        }
    }
}
