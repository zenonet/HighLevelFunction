namespace Hlf.Transpiler;

public class LanguageException(string customErrorMessage, int line, int column) : Exception
{

   public LanguageException(string customErrorMessage, Token token) : this(customErrorMessage, token.Line, token.Column)
   {
   }
   public string CustomErrorMessage { get; } = customErrorMessage;
   public int Line { get; } = line;
   public int Column { get; } = column;

   public override string ToString()
   {
      return $"Error (l:{Line}{(Column != -1 ? $",c:{Column}" : "")}): {CustomErrorMessage}";
   }
}