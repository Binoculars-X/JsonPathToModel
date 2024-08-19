using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JsonPathToModel;

public static class JsonExtensions
{
    public static object? Deserialize(Type type, string json)
    {
        MethodInfo method = typeof(JsonSerializer)
            .GetMethods()
            .FirstOrDefault(x => x.Name == "Deserialize" && x.IsGenericMethod && x.MetadataToken == 100664404)!;

        MethodInfo generic = method.MakeGenericMethod(type);
        var obj = generic.Invoke(null, [json, null]);
        return obj;
    }
}

// https://www.quora.com/In-C-can-you-add-extension-methods-to-an-existing-static-class
//public static class MyStaticClass
//{
//    public static int MyStaticMethod()
//    {
//        return 42;
//    }
//}

//public static class MyStaticClassExtensions
//{
//    public static int MyExtensionMethod(this MyStaticClass self)
//    {
//        return self.MyStaticMethod() + 1;
//    }
//}
