using System;
using System.Collections.Generic;
using System.Text;

namespace JsonPathToModel.Parser;

public class ParserException : Exception
{
    public ParserException(string message) : base(message)
    { }
}
