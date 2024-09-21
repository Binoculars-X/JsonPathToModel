using JsonPathToModel.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests.Extensions;

public class HackingExtensionsTests
{
    [Fact]
    public void HackingExtensions_Should_Update_Privates()
    {
        var target = new HackingExtensionsTestsSample();
        target.WithHack("_privateProperty", "test1");
        target.WithHack("_private", "test2");

        Assert.Equal("test1", target.StealValue("_privateProperty"));
        Assert.Equal("test2", target.StealValue("_private"));
    }

    [Fact]
    public void HackingExtensions_Should_Update_Readonly()
    {
        var target = new HackingExtensionsTestsSample();
        target.WithHack("_readonlyDependency", "test");
        Assert.Equal("test", target.StealValue("_readonlyDependency"));
    }

    [Fact]
    public void HackingExtensions_Should_Update_PrivateSet()
    {
        var target = new HackingExtensionsTestsSample();
        target.WithHack("_privateSetProperty", "test");
        Assert.Equal("test", target.StealValue("_privateSetProperty"));
    }

    [Fact]
    public void HackingExtensions_Should_Update_Internal()
    {
        var target = new HackingExtensionsTestsSample();
        target.WithHack("_internal", "test");
        Assert.Equal("test", target.StealString("_internal"));
    }

    [Fact]
    public void HackingExtensions_Should_Update_Protected_FullNotation()
    {
        var target = new HackingExtensionsTestsSample();
        target.WithHack("$._protected", "test");
        Assert.Equal("test", target.StealString("$._protected"));
    }

    [Fact]
    public void HackingExtensions_Should_Update_Protected()
    {
        var target = new HackingExtensionsTestsSample();
        target.WithHack("_protected", "test");
        Assert.Equal("test", target.StealString("_protected"));
    }

    [Fact]
    public void HackingExtensions_ShouldNot_Update_GetProperty()
    {
        var target = new HackingExtensionsTestsSample();
        var exc = Assert.Throws<NavigationException>(() =>  target.WithHack("_getProperty", "test"));
        Assert.Contains("_getProperty", exc.Message);
    }
}

public class HackingExtensionsTestsSample
{
    private readonly object? _readonlyDependency = null;
    private object? _privateProperty { get; set; } = null;
    private object? _private = null;
    public object? _privateSetProperty { get; private set; } = null;
    internal object? _internal = null;
    protected object? _protected = null;
    public object? _getProperty { get; } = null;
}
