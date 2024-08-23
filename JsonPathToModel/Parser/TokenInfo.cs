using System;
using System.Collections.Generic;
using System.Text;
using static JsonPathToModel.Parser.Tokenizer;

namespace JsonPathToModel.Parser;

public class TokenInfo
{
    public Token Token { get; set; }
    public string Field { get; set; }
    public CollectionDef? Collection { get; set; }
    //public string Operator { get; set; }
    //public string Param { get; set; }
    //public string StringLiteral { get; set; }
    //public decimal Number { get; set; }
    //public List<TokenInfo> Expression { get; set; }

    //public void Print(StringBuilder sb)
    //{
    //    switch (Token)
    //    {
    //        case Token.Param: sb.Append($"{Param}"); break;
    //        case Token.Number: sb.Append($"{Number}"); break;
    //        case Token.Operator: sb.Append($"{Operator}"); break;
    //        case Token.StringLiteral: sb.Append($"'{StringLiteral}'"); break;
    //        case Token.Field: sb.Append($"{Field}"); break;
    //    }
    //}
}

public enum Token
{
    EOF,
    Dollar,
    Operator,
    Field,
    Param,
    OpenParens,
    CloseParens,

    Add,
    Subtract,
    Multiply,
    Divide,
    Dot,
    Comma,
    //Identifier,
    Number,
    StringLiteral,

    // used for expressions
    ParensExpression,
    MathExpression
}

