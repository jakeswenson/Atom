using System;
using CommandLine;
using HadronApplication.Options;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace HadronApplication
{
    public class Hadron
    {
        public static int Main(string[] args)
        {
            try
            {

                Initialize();

                var options = new HadronOptions();

                var verbs = Verb.Parse(args, options);

                if (verbs == null)
                {
                    return Parser.DefaultExitCodeFail;
                }

                // Hadron code
                foreach (var verb in verbs)
                {
                    verb.Run();
                }

                Log.Information($"{nameof(Hadron)} done");
                return 0;
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error running {nameof(Hadron)}");
                return -1;
            }
        }

        private static void Initialize()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.ColoredConsole(
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    outputTemplate: "{Message}{NewLine}{Exception}")
                .CreateLogger();
        }
    }
}