namespace Skywithin.AutoRegister.DI;

internal static class ConsoleHelper
{
    internal static void PrintError(string message) => Print(message, ConsoleColor.Red);

    internal static void PrintSuccess(string message) => Print(message, ConsoleColor.Green);

    internal static void PrintWarning(string message) => Print(message, ConsoleColor.Yellow);

    internal static void PrintInfo(string message) => Print(message, ConsoleColor.Cyan);

    internal static void Print(string message, ConsoleColor color = ConsoleColor.Gray)
    {
        var originalColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = originalColor;
    }

    internal static void NewLine() => Console.WriteLine();
}
