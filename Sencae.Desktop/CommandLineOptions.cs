using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace Sencae; 

public class CommandLineOptions {
    [Option(
        'r',
        "rom",
        HelpText = "Path to NES ROM file",
        //Default = "",
        SetName = "Source",
        Required = true
    )]
    public string RomFilePath { get; set; }

    [Usage(ApplicationAlias = "NesCore")]
    public static IEnumerable<Example> Examples {
        get {
            yield return new Example(
                "Open ROM from file",
                new CommandLineOptions { RomFilePath = "game.nes" });
        }
    }

    public CommandLineOptions()
    {

    }
}