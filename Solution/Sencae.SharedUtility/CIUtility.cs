namespace Sencae.SharedUtility;

public static class CIUtility {
    public static readonly bool IsRunningUnderCI =
        Environment.GetEnvironmentVariable("CI") != "1" ||
        !String.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("GITHUB_ENV"));
}
