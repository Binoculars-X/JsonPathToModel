using System;
using System.Collections.Generic;
using System.Text;
using JsonPathToModel.Parser;

namespace JsonPathToModel.Interfaces;

public record ExpressionResult(
    string Expression,
    List<TokenInfo> Tokens,
    Func<object, object>? GetDelegate,
    Action<object, object>? SetDelegate)
{
    public readonly bool ContainsCollections = Tokens.Any(t => t.Collection != null);
};
