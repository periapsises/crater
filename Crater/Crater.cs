using Spectre.Console.Cli;

namespace Crater;

public static class Crater
{
    public static int Main(string[] args)
    {
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.SetApplicationName("crater");
            config.AddCommand<CompileCommand>("compile")
                .WithDescription("Compile crater source files to Lua.");

#if DEBUG
            config.PropagateExceptions();
#endif
        });

        return app.Run(args);
    }
}
