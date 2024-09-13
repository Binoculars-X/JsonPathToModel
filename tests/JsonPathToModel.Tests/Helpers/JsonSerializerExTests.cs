using JsonPathToModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests.Helpers;

public class JsonSerializerExTests
{
    [Fact]
    public void JsonSerializerEx_Should_DeserializeFromKnownType()
    {
        var model = new Model()
        {
            Id = 7,
            Name = "Text",
            Records = { { "One", new ModelRecord() { Id = 12, Name = "Nested Text" } } }
        };

        var json = JsonSerializer.Serialize(model);

        var restored = JsonSerializerEx.Deserialize(json, model.GetType().FullName!) as Model;

        Assert.Equal(model.Id, restored!.Id);
        Assert.Equal(model.Name, restored.Name);
        Assert.Equal(model.Records["One"].Id, restored.Records["One"].Id);
        Assert.Equal(model.Records["One"].Name, restored.Records["One"].Name);
    }

    [Fact]
    public void SnapshotRecord_Should_DeserializeFromKnownType()
    {
        var model = new Model()
        {
            Id = 7,
            Name = "Text",
            Records = { { "One", new ModelRecord() { Id = 12, Name = "Nested Text" } } }
        };

        var json = JsonSerializer.Serialize(model);
        var record = new SnapshotRecord(model.GetType().FullName!, json);
        var restored = record.Deserialize() as Model;

        Assert.Equal(model.Id, restored!.Id);
        Assert.Equal(model.Name, restored.Name);
        Assert.Equal(model.Records["One"].Id, restored.Records["One"].Id);
        Assert.Equal(model.Records["One"].Name, restored.Records["One"].Name);
    }

    [Fact]
    public void SnapshotRecord_Should_DeserializeFromModel()
    {
        var model = new Model()
        {
            Id = 7,
            Name = "Text",
            Records = { { "One", new ModelRecord() { Id = 12, Name = "Nested Text" } } }
        };

        var record = new SnapshotRecord(model);
        var restored = record.Deserialize() as Model;

        Assert.Equal(model.Id, restored!.Id);
        Assert.Equal(model.Name, restored.Name);
        Assert.Equal(model.Records["One"].Id, restored.Records["One"].Id);
        Assert.Equal(model.Records["One"].Name, restored.Records["One"].Name);
    }

    [Fact]
    public void SnapshotRecord_ShouldBe_Deserializable()
    {
        var model = new Model()
        {
            Id = 7,
            Name = "Text",
            Records = { { "One", new ModelRecord() { Id = 12, Name = "Nested Text" } } }
        };

        var record = new SnapshotRecord(model);
        var json = JsonSerializer.Serialize(record);
        var restoredRecord = JsonSerializerEx.Deserialize(json, record.GetType().FullName!) as SnapshotRecord;

        Assert.Equal(record.Json, restoredRecord.Json);
        Assert.Equal(record.Type, restoredRecord.Type);

        var restoredModel = restoredRecord.Deserialize() as Model;
        Assert.Equal(model.Id, restoredModel!.Id);
        Assert.Equal(model.Name, restoredModel.Name);
        Assert.Equal(model.Records["One"].Id, restoredModel.Records["One"].Id);
        Assert.Equal(model.Records["One"].Name, restoredModel.Records["One"].Name);
    }
}

public class Model
{
    public int Id { get; set; }
    public string? Name { get; set; }

    public Dictionary<string, ModelRecord> Records { get; set; } = [];
}

public class ModelRecord
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

