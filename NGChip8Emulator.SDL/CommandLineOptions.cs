using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace NGChip8Emulator.nativelibs {
    public class CommandLineOptions {
        [Option(
            'r',
            "rom",
            HelpText = "Path to CHIP8 ROM file",
            //Default = "",
            SetName = "Source",
            Required = true
        )]
        public string RomFilePath { get; private set; }

        [Usage(ApplicationAlias = "LostPolygon.MethodInlineInjector.Console")]
        public static IEnumerable<Example> Examples {
            get {
                yield return new Example(
                    "Open ROM from file",
                    new CommandLineOptions { RomFilePath = "game.ch8" });
            }
        }
    }
}
