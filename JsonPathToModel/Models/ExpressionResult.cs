using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using JsonPathToModel.Parser;

namespace JsonPathToModel;

internal record ExpressionResult(
    string Expression,
    List<TokenInfo> Tokens,
    Func<object, object>? GetDelegate,
    SetDelegateDetails? SetDelegateDetails
    )
{
    public readonly bool ContainsCollections = Tokens.Any(t => t.CollectionDetails != null);
};

internal record SetDelegateDetails(PropertyInfo TargetProperty, Action<object, object?> SetDelegate);
