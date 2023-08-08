using System.Text.RegularExpressions;
using Sixty502DotNet.Shared;
using Environment = Sixty502DotNet.Shared.Environment;

namespace Senge.Tests;

public static class AssemblerUtility {
    public static byte[] AssembleFromListing(string sourceListing) {
        Environment.Reset();

        Interpreter interpreter = new(new Options {
            ArchitectureOptions = new ArchitectureOptions {
                Cpuid = "6502"
            },
            GeneralOptions = new GeneralOptions {
            },
            DiagnosticOptions = new DiagnosticOptions {
            },
            OutputOptions = new OutputOptions {
                NoDisassembly = true,
            }
        }, new FileSystemBinaryReader(null));
        
        string truncatedListing =
            String.Join(
                "\n",
                sourceListing
                    .Split("\n", StringSplitOptions.TrimEntries)
                    .Select(s => s.Trim())
                    .Select(s => {
                        Match match = Regex.Match(s, "(?<!^)([A-Z]){3}", RegexOptions.Singleline);
                        return s[match.Index..];
                    })
            ).Trim();

        AssemblyState assemblyState = interpreter.Exec(truncatedListing, new StringSourceFactory());
        assemblyState.Warnings.ForEach(w => Console.WriteLine($"[Asm Warning] {w}"));
        if (assemblyState.Errors.Any()) {
            assemblyState.Errors.ForEach(w => Console.WriteLine($"[Asm Error] {w}"));
            throw new Exception("Assembly failed");
        }
        
        Console.WriteLine("=== Asm Start");
        assemblyState.StatementListings
            .Select(s => {
                Match match = Regex.Match(s, "(?<!^)([A-Z]){3}", RegexOptions.Singleline);
                if (!match.Success) {
                    match = Regex.Match(s, "(?<!^)([A-Za-z]){3,}", RegexOptions.Singleline);
                }
                return s[..(match.Index - 34)] + s[match.Index..];
            })
            .ToList()
            .ForEach(Console.WriteLine);
        Console.WriteLine("=== Asm End");

        return assemblyState.Output.GetCompilation().ToArray();
    }
}
