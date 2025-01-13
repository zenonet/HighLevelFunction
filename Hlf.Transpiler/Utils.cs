using System.Text;
using System.Text.RegularExpressions;

namespace Hlf.Transpiler;

public static partial class Utils
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
    
    public static void AppendWithPrefix(this StringBuilder builder, string prefix, string commands)
    {
        if (string.IsNullOrWhiteSpace(commands)) return;
        if (builder.Length > 0 && builder[^1] != '\n') builder.Append('\n');
        builder.Append(CommandPrefixRegex().Replace(commands, $"{prefix} $&"));
    }
    

    public static void SmartAppendL(this StringBuilder builder, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        builder.AppendLine(value);
    }
    public static void SmartAppend(this StringBuilder builder, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        builder.Append(value);
    }
    public static void AppendFree(this StringBuilder builder, string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        string s = CommandPrefixRegex().Replace(value, ";fr:$&");
        builder.Append(s);
    }

    public static string Free(this string freeStatement) => $";fr:{freeStatement}";
    
    [GeneratedRegex(@"^\w.*", RegexOptions.Multiline)]
    public static partial Regex CommandPrefixRegex();
}