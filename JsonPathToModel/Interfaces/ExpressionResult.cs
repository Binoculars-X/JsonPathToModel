using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using JsonPathToModel.Parser;

namespace JsonPathToModel.Interfaces;

internal record ExpressionResult(
    string Expression,
    List<TokenInfo> Tokens,
    Func<object, object>? GetDelegate,
    //Action<object, object>? SetDelegate
    SetDelegateDetails? SetDelegateDetails
    )
{
    public readonly bool ContainsCollections = Tokens.Any(t => t.CollectionDetails != null);
};

internal record SetDelegateDetails(PropertyInfo TargetProperty, Action<object, object?> SetDelegate);
