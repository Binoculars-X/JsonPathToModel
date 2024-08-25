using System;
using System.Collections.Generic;
using System.Text;

namespace JsonPathToModel.Parser;

public record ExpressionResult(string Expression, List<TokenInfo> Tokens, Func<object, object>? GetDelegate)
{
    public readonly bool ContainsCollections = Tokens.Any(t => t.Collection != null);
};
