using System;
using System.Reflection;
using CommandLine;
using CommandLine.Text;

namespace Sencae {
    public class EmulatorCommandLineHandler {
        private readonly string[] _args;
        private CommandLineOptions _commandLineOptions;
        private ParserResult<CommandLineOptions> _commandLineParserResult;

        public static void Run(string[] args) {
            EmulatorCommandLineHandler handler = new EmulatorCommandLineHandler(args);
            handler.Run();
        }

        private EmulatorCommandLineHandler(string[] args) {
            _args = args;
        }

        private void Run() {
            new EmulatorRunner(new CommandLineOptions
            {
                RomFilePath = _args[0]
            }).Run();
            return;
            Parser commandLineParser =
                new Parser(settings => {
                    settings.IgnoreUnknownArguments = false;
                    //settings.CaseInsensitiveEnumValues = true;
                    settings.CaseSensitive = false;
                    //settings.MaximumDisplayWidth = 100;
                    settings.HelpWriter = null;
                });

            _commandLineParserResult = commandLineParser.ParseArguments<CommandLineOptions>(_args);
            _commandLineParserResult
                .WithParsed(options => {
                    _commandLineOptions = options;
                    new EmulatorRunner(_commandLineOptions).Run();
                })
                .WithNotParsed(errors => {
                    Console.WriteLine(GetHelpText());
                    Environment.ExitCode = 1;
                });
        }

        private void ShowHelpTextWithCustomErrorMessage(params string[] errors) {
            HelpText helpText = GetHelpText();
            Console.WriteLine(helpText.Heading);
            Console.WriteLine(helpText.Copyright);
            helpText.Heading = "";
            helpText.Copyright = "";
            Console.WriteLine();
            Console.WriteLine(helpText.SentenceBuilder.ErrorsHeadingText());
            foreach (string error in errors) {
                Console.WriteLine(error);
            }

            Console.WriteLine(helpText);
        }

        private HelpText GetHelpText() {
            Assembly assembly = Assembly.GetExecutingAssembly();

            string assemblyConfiguration = assembly.GetCustomAttribute<AssemblyConfigurationAttribute>()?.Configuration;
            string assemblyVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

            HelpText helpText =
                _commandLineParserResult.Tag == ParserResultType.NotParsed ?
                    HelpText.AutoBuild(_commandLineParserResult) :
                    HelpText.AutoBuild(_commandLineParserResult, text => text, example => example);
            helpText.AddEnumValuesToHelpText = true;
            helpText.AddDashesToOption = true;
            helpText.Heading = $"NesCore v{assemblyVersion} ({assemblyConfiguration}";
            helpText.Copyright = "© Lost Polygon";
            return helpText;
        }
    }

}
