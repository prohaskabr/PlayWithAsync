namespace PlayWithAsync;
public abstract class Food
{
    private readonly TimeSpan cookTime;

    public string Name { get; set; }
    protected Food(TimeSpan cookTime)
    {
        this.cookTime = cookTime;
        Name = GetType().Name;
    }

    public async Task Cook()
    {

        Console.WriteLine($"Cooking {Name}");

        await Task.Delay(cookTime);

        Console.WriteLine($"{Name} Completed");

    }
}

public class Turkey(): Food(TimeSpan.FromSeconds(5));
public class MashedPotatoes() : Food(TimeSpan.FromSeconds(2));
public class Gravy() : Food(TimeSpan.FromSeconds(1));
public class Stuffing() : Food(TimeSpan.FromSeconds(2));
