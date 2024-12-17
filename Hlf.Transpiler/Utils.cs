using System.Text;

namespace Hlf.Transpiler;

public static class Utils
{
    public static string GetOrdinal(this int i)
    {
        if (i == 1) return "1st";
        if (i == 2) return "2nd";
        if (i == 3) return "3rd";
        return $"{i}th";
    }

    public static void AppendCommands(this StringBuilder builder, Scope scope, string commands)
    {
        if (string.IsNullOrWhiteSpace(commands)) return;
        if (builder.Length > 0 && builder[^1] != '\n') builder.Append('\n');
        builder.Append(scope.ApplyScopedPrefixes(commands));
    }
}