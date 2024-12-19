namespace Hlf.Transpiler;

public class LanguageException(string customErrorMessage, int line, int column, int length = 1) : Exception
{

   public LanguageException(string customErrorMessage, Token token) : this(customErrorMessage, token.Line, token.Column, token.Content.Length)
   {
   }
   public string CustomErrorMessage { get; } = customErrorMessage;
   public int Line { get; } = line;
   public int Column { get; } = column;
   public int Length { get; set; } = length;

   public override string ToString()
   {
      return $"Error ({Line}{(Column != -1 ? $",{Column}" : "")}): {CustomErrorMessage}";
   }
}