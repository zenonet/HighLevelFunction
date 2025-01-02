using Hlf.Transpiler.CodeGen;

namespace Hlf.Transpiler;

public class SelectorExpression : Statement
{
    public string Selector;
    public override void Parse()
    {
        // If the selector might select multiple entities
        if (!"spr".Contains(Selector[1]))
        {
            
            bool hasLimit = false;
            int offset = 3;

            void SkipWhiteSpace()
            {
                while (offset < Selector.Length && char.IsWhiteSpace(Selector[offset])) offset++;
            }
            while (offset < Selector.Length-1)
            {
                SkipWhiteSpace();
                if (Selector[offset..].StartsWith("limit"))
                {
                    offset += 5;
                    SkipWhiteSpace();
                    if (Selector[offset++] != '=') throw new LanguageException("Expected '=' in selector", Line, Column+offset, 1);
                    if(Selector[offset++] != '1' || char.IsDigit(Selector[offset])) throw new LanguageException("Selector has to have a limit of 1 to guarantee only one entity will be selected.", Line, Column+offset, 1);
                    SkipWhiteSpace();
                    if(Selector[offset++] is not ',' and not ']') throw new LanguageException("Expected comma or end of selector in selector after limit", Line, Column+offset, 1);
                    hasLimit = true;
                    continue;
                }
                while(char.IsLetter(Selector[offset])) offset++;
                SkipWhiteSpace();
                if (Selector[offset++] != '=') throw new LanguageException("Expected '=' in selector", Line, Column+offset, 1);
                SkipWhiteSpace();
                int opened = 0;
                while (opened >= 0)
                {
                    if(Selector[offset] == ',' && opened == 0) break;
                    if(Selector[offset] == '[') opened++;
                    if(Selector[offset] == ']') opened--;
                    offset++;
                }
                if(opened == -1) break;

                offset++;
            }
            if(!hasLimit) throw new LanguageException("Expected limit of 1 in selector to ensure the selector only matches one entity", Line, Column, Selector.Length);
        }
        
        Result = DataId.FromType(HlfType.Entity);
    }

    public override string Generate(GeneratorOptions options)
    {
        return $"tag {Selector} add {Result.Generate(options)}";
    }
}