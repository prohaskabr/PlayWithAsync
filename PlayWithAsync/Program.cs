// See https://aka.ms/new-console-template for more information
using PlayWithAsync;

Console.WriteLine("Cooking Started");


var turkey = new Turkey();
var gravy = new Gravy();

await Task.WhenAll(turkey.Cook(), gravy.Cook());

Console.WriteLine("Ready to eat");