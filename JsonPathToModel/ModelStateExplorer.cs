using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

namespace JsonPathToModel;

public class ModelStateExplorer : IModelStateExplorer
{
    public ModelStateResult FindModelStateItems(Type modelType, ModelSearchParams ps)
    {
        if (ps.Nested)
        {
            throw new NotImplementedException("Nested members not implemented yet");
        }

        var list = new List<ModelStateItem>();
        var bindings = BindingFlags.Instance;

        if (ps.Public)
        {
            bindings |= BindingFlags.Public;
        }

        if (ps.NonPublic)
        {
            bindings |= BindingFlags.NonPublic;
        }

        var fields = modelType.GetFields(bindings)
            // igonore generated property back fields
            .Where(f => !f.Name.StartsWith("<") && f.GetCustomAttribute<JsonIgnoreAttribute>() == null)
            .ToList();
        
        // include only properties with setters
        var properties = modelType.GetProperties(bindings)
            .Where(p => p.CanWrite && p.GetCustomAttribute<JsonIgnoreAttribute>() == null)
            .ToList();

        if (ps.ExcludeStartsWith != null)
        {
            fields = fields.Where(f => !f.Name.StartsWith(ps.ExcludeStartsWith)).ToList();
            properties = properties.Where(p => !p.Name.StartsWith(ps.ExcludeStartsWith)).ToList();
        }

        foreach (var field in fields)
        {
            list.Add(new ModelStateItem(field.FieldType, $"$.{field.Name}", field, null));
        }

        foreach (var property in properties)
        {
            list.Add(new ModelStateItem(property.PropertyType, $"$.{property.Name}", null, property));
        }

        return new ModelStateResult(modelType.FullName, list);
    }
}
