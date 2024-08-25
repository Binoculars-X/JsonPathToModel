using System;
using System.Collections.Generic;
using System.Text;

namespace JsonPathToModel.Exceptions;

public class NavigationException : Exception
{
    public NavigationException(string message) : base(message)
    { }
}
