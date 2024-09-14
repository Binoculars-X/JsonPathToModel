using Castle.DynamicProxy;
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

public partial class ModelSnapshotTests
{
    //[Fact]
    public void ModelSnapshot_Should_Import_And_Export_PrivatePropertiesOfProxy()
    {
        var date = new DateTime(1970, 10, 19);
        var model = new ModelSnapshot();

        var target = new SamplePrivateFieldsModel();
        target._internalString = "1";


        var proxy = new ProxyGenerator().CreateClassProxyWithTarget(classToProxy: target.GetType(),
                constructorArguments: null,
                target: target,
                options: new ProxyGenerationOptions(),
                interceptors: []) as SamplePrivateFieldsModel;

        proxy._internalString = "2";

        model.ImportFrom(target, _importOptions);
        Assert.Equal(4, model.Records.Count());

        model.ImportFrom(proxy, _importOptions);
        Assert.Equal(4, model.Records.Count());
    }
}

public class SamplePrivateFieldsModel
{
    public SamplePrivateFieldsModel()
    {
        _modelDate = DateTime.UtcNow;
        _modelString = "aha";
    }

    private DateTime? _modelDate;
    private int? _modelInt;
    private string? _modelString { get; set; }

    internal string? _internalString { get; set; }

    public DateTime? AccessorModelDate { get { return _modelDate; } }
    //public int? AccessorModelInt { get { return _modelInt; } }
    //public string? AccessorModelString { get { return _modelString; } }
}





