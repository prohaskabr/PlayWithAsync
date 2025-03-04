// See https://aka.ms/new-console-template for more information


using TaskFromScratch;

Console.WriteLine($"Starting thread Id: {Environment.CurrentManagedThreadId}");

//MyCustomTask.Run(() => Console.WriteLine($"First MyCustomTask thread Id: {Environment.CurrentManagedThreadId}"))
//    .ContinueWith(() => Console.WriteLine($"Second MyCustomTask thread Id: {Environment.CurrentManagedThreadId}"));

var task = MyCustomTask.Run(() =>
{
    Console.WriteLine($"First MyCustomTask thread Id: {Environment.CurrentManagedThreadId}");
});

task.ContinueWith(() =>
{
    MyCustomTask.Run(() =>
    {
        Console.WriteLine($"Third MyCustomTask thread Id: {Environment.CurrentManagedThreadId}");
    });

    Console.WriteLine($"Second MyCustomTask thread Id: {Environment.CurrentManagedThreadId}");
});


Console.ReadLine();