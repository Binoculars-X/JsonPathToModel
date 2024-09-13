using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using JsonPathToModel.Helpers;

namespace JsonPathToModel;

public static class ModelSnapshotExtensions
{
    private readonly static JsonPathModelNavigator _navi = new JsonPathModelNavigator(new() { OptimizeWithCodeEmitter = true });
    private readonly static ModelStateExplorer _expl = new();

    public static void ExportTo(this ModelSnapshot model, object target)
    {
        try
        {
            foreach (var item in model.Records)
            {
                var mapping = item.Key;
                var record = item.Value;
                var value = JsonSerializerEx.Deserialize(record.Json, record.Type);

                _navi.SetValue(target, mapping, value);
            }
        }
        catch (Exception exc)
        {
            throw;
        }
    }

    public static void ImportFrom(this ModelSnapshot model, object target, ImportOptions? options = null)
    {
        model.Clear();
        var ps = ModelSearchParams.FullModelState();

        if (options?.ExcludeStartsWith != null)
        { 
            ps.ExcludeStartsWith = options.ExcludeStartsWith;
        }

        var stateResult = _expl.FindModelStateItems(target.GetType(), ps);

        foreach (var item in stateResult.Items)
        {
            var mapping = item.JsonPath;
            var type = item.Type;
            var value = _navi.GetValue(target, mapping);
            var json = JsonSerializer.Serialize(value);
            model.Records[mapping] = new SnapshotRecord(type.FullName, json);
        }
    }

    public static object? Deserialize(this SnapshotRecord record)
    {
        var value = JsonSerializerEx.Deserialize(record.Json, record.Type);
        return value;
    }
}

public class ImportOptions
{
    public string? ExcludeStartsWith { get; set; }
}
