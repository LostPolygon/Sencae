namespace System.Runtime.CompilerServices;

using global::System.Diagnostics;
using global::System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage, DebuggerNonUserCode]
internal static class IsExternalInit {
}

internal class RequiredMemberAttribute : Attribute;

internal class CompilerFeatureRequiredAttribute : Attribute {
    public CompilerFeatureRequiredAttribute(string name) {
    }
}
