using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonPathToModel;

/// <summary>
/// Stores model public and nonpublic fields and properties in text form (json)
/// Fully serializable
/// Intended to store model state data between sessions
/// </summary>
public class ModelSnapshot
{
    public Dictionary<string, SnapshotRecord> Records { get; set; } = [];

    public void Clear()
    {
        Records.Clear();
    }
}

/// <summary>
/// Model single field/property store
/// Keeps property data type name and serialized value
/// Fully serializable
/// </summary>
/// <param name="Type"></param>
/// <param name="Json"></param>
public record SnapshotRecord(string? Type, string? Json)
{
    public SnapshotRecord() : this(null, null)
    {
    }

    public SnapshotRecord(object model) :
        this(model.GetType().FullName, JsonSerializer.Serialize(model))
    {
    }
}
