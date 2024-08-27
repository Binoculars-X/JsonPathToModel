using System;
using System.Collections.Generic;
using System.Text;
using static JsonPathToModel.Parser.Tokenizer;

namespace JsonPathToModel.Parser;

internal class TokenInfo
{
    public Token Token { get; set; }
    public string? Field { get; set; }
    public CollectionDef? CollectionDetails { get; set; }
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

