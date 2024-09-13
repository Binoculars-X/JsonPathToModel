using JsonPathToModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests.Helpers;

public class ModelSnapshotTests
{
    [Fact]
    public void ModelSnapshot_Should_Import_PrimitiveValues()
    {
        var date = new DateTime(1970, 10, 19);
        var model = new ModelSnapshot();

        var target = new SampleTarget()
        {
            Id = "ORDER-123",
            ModelInt = 7,
            ModelDate = date,
            ModelString = "Text"
        };

        model.ImportFrom(target);

        var idRecord = model.Records["$.Id"];
        Assert.Equal(target.Id, JsonSerializerEx.Deserialize(idRecord.Json, idRecord.Type));
        var modelIntRecord = model.Records["$.ModelInt"];
        Assert.Equal(target.ModelInt, JsonSerializerEx.Deserialize(modelIntRecord.Json, modelIntRecord.Type));
        var modelDate = model.Records["$.ModelDate"];
        Assert.Equal(target.ModelDate, JsonSerializerEx.Deserialize(modelDate.Json, modelDate.Type));
        var modelString = model.Records["$.ModelString"];
        Assert.Equal(target.ModelString, JsonSerializerEx.Deserialize(modelString.Json, modelString.Type));
    }

    [Fact]
    public void ModelSnapshot_Should_Import_And_Export_PrimitiveValues()
    {
        var date = new DateTime(1970, 10, 19);
        var model = new ModelSnapshot();

        var target = new SampleTarget()
        {
            Id = "ORDER-123",
            ModelInt = 7,
            ModelDate = date,
            ModelString = "Text"
        };

        model.ImportFrom(target);
        var target2 = new SampleTarget();
        model.ExportTo(target2);

        Assert.Equal(target.Id, target2.Id);
        Assert.Equal(target.ModelInt, target2.ModelInt);
        Assert.Equal(target.ModelDate, target2.ModelDate);
        Assert.Equal(target.ModelString, target2.ModelString);
    }

    [Fact]
    public void ModelSnapshot_Should_Support_JsonIgnore()
    {
        var date = new DateTime(1970, 10, 19);
        var model = new ModelSnapshot();

        var target = new SampleTargetJsonIgnore()
        {
            Id = "ORDER-123",
            ModelInt = 7,
            ModelDate = date,
            ModelString = "Text"
        };

        model.ImportFrom(target);
        Assert.Equal(3, model.Records.Count);

        var target2 = new SampleTargetJsonIgnore();
        model.ExportTo(target2);

        Assert.Equal(target.Id, target2.Id);
        Assert.Equal(target.ModelInt, target2.ModelInt);
        Assert.Null(target2.ModelDate);
        Assert.Equal(target.ModelString, target2.ModelString);
    }

    [Fact]
    public void ModelSnapshot_Should_Support_Exclusion()
    {
        var date = new DateTime(1970, 10, 19);
        var model = new ModelSnapshot();

        var target = new SampleTargetExclude()
        {
            __Id = "ORDER-123",
            ModelInt = 7,
            ModelDate = date,
            ModelString = "Text"
        };

        model.ImportFrom(target, new ImportOptions { ExcludeStartsWith = "__" });
        Assert.Equal(3, model.Records.Count);

        var target2 = new SampleTargetExclude();
        model.ExportTo(target2);

        Assert.Null(target2.__Id);
        Assert.Equal(target.ModelInt, target2.ModelInt);
        Assert.Equal(target.ModelDate, target2.ModelDate);
        Assert.Equal(target.ModelString, target2.ModelString);
    }

    [Fact]
    public void ModelSnapshot_Should_Export_ToAnotherType()
    {
        var date = new DateTime(1970, 10, 19);
        var model = new ModelSnapshot();

        var target = new SampleTarget()
        {
            Id = "ORDER-123",
            ModelInt = 7,
            ModelDate = date,
            ModelString = "Text"
        };

        model.ImportFrom(target);
        Assert.Equal(4, model.Records.Count);

        var target2 = new SampleTargetJsonIgnore();
        model.ExportTo(target2);

        Assert.Equal(target.Id, target2.Id);
        Assert.Equal(target.ModelInt, target2.ModelInt);
        Assert.Equal(target.ModelDate, target2.ModelDate);
        Assert.Equal(target.ModelString, target2.ModelString);
    }

    [Fact]
    public void ModelSnapshot_Should_Import_And_Export_PrivateFieldsAndProperties()
    {
        var date = new DateTime(1970, 10, 19);
        var model = new ModelSnapshot();

        var target = new SamplePrivateTarget(date, 7, "Text", "ORDER-123", "lala");

        model.ImportFrom(target);
        var target2 = new SamplePrivateTarget();
        model.ExportTo(target2);

        Assert.Equal(target.Id, target2.Id);
        Assert.Equal(target.ModelInt, target2.ModelInt);
        Assert.Equal(target.ModelDate, target2.ModelDate);
        Assert.Equal(target.ModelString, target2.ModelString);
        Assert.Equal(target.StringProp, target2.StringProp);
    }

    public class SampleTarget
    {
        public DateTime? ModelDate { get; set; }
        public int? ModelInt { get; set; }
        public string? ModelString { get; set; }
        public string? Id { get; set; }
    }

    public class SampleTargetExclude
    {
        public DateTime? ModelDate { get; set; }
        public int? ModelInt { get; set; }
        public string? ModelString { get; set; }
        public string? __Id { get; set; }
    }

    public class SampleTargetJsonIgnore
    {
        [JsonIgnore]
        public DateTime? ModelDate { get; set; }

        public int? ModelInt { get; set; }
        public string? ModelString { get; set; }
        public string? Id { get; set; }
    }

    public class SamplePrivateTarget
    {
        public SamplePrivateTarget() { }

        public SamplePrivateTarget(DateTime? modelDate, int? modelInt, string? modelString, string? id, string? stringProp)
        {
            _ModelDate = modelDate;
            _ModelInt = modelInt;
            _ModelString = modelString;
            _Id = id;
            _stringProp = stringProp;
        }

        private DateTime? _ModelDate;
        private int? _ModelInt;
        private string? _ModelString;
        private string? _Id;

        private string? _stringProp { get; set; }

        public DateTime? ModelDate { get { return _ModelDate; } }

        public int? ModelInt { get { return _ModelInt; } }
        public string? ModelString { get { return _ModelString; } }
        public string? Id { get { return _Id; } }
        public string? StringProp { get { return _stringProp; } }
    }
}



