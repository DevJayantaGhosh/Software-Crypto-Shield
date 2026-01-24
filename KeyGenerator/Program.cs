using Spectre.Console.Cli;
using KeyGenerator.Commands;

namespace KeyGenerator;

public class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.SetApplicationName("cryptoshield-keygen");
            config.AddCommand<GenerateCommand>("generate")
                  .WithDescription("Generate RSA or ECDSA key pairs");
        });

        return app.Run(args);
    }
}
