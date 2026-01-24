using Spectre.Console;

namespace SoftwareSigner.Utilities;

public static class CliSpinner
{
    public static void Run(bool enabled, string message, Action action)
    {
        if (!enabled)
        {
            action();
            return;
        }

        AnsiConsole.Status()
            .Spinner(Spinner.Known.Material)
            .SpinnerStyle(Style.Parse("cyan"))
            .Start(message, _ => action());
    }
}
