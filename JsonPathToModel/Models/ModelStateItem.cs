using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace JsonPathToModel;

public record ModelStateItem(Type Type, string JsonPath, FieldInfo? Field, PropertyInfo? Property);
