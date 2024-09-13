using JsonPathToModel.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests.Explorer;

public class ModelStateExplorerExplorerTests
{
    [Fact]
    public void Should_FindNonPublic_Members()
    {
        var expl = new ModelStateExplorer();
        var result = expl.FindModelStateItems(typeof(NonPublicMembers), ModelSearchParams.FullModelState());

        Assert.Equal(8, result.Items.Count());
    }

    [Fact]
    public void Should_IgnorePropertyWithoutSetter()
    {
        var expl = new ModelStateExplorer();
        var result = expl.FindModelStateItems(typeof(NoSetterProperty), ModelSearchParams.FullModelState());

        Assert.Empty(result.Items);
    }

    [Fact]
    public void Should_Support_JsonIgnore()
    {
        var expl = new ModelStateExplorer();
        var result = expl.FindModelStateItems(typeof(NonPublicMembersJsonIgnore), ModelSearchParams.FullModelState());

        Assert.Equal(6, result.Items.Count());
    }

    [Fact]
    public void Should_Support_DI()
    {
        var expl = ConfigHelper.GetConfigurationServices().GetService<IModelStateExplorer>();
        Assert.NotNull(expl);
    }

    public class NonPublicMembers
    {
        private int _id;
        protected string _name;
        internal DateTime _created;
        public decimal Updated;

        private int Id { get; set; }
        protected string Name { get; set; }
        internal DateTime Created { get; set; }
        public decimal UpdatedProp { get; set; }
    }

    public class NonPublicMembersJsonIgnore
    {
        private int _id;
        protected string _name;

        [JsonIgnore]
        internal DateTime _created;

        public decimal Updated;

        [JsonIgnore]
        private int Id { get; set; }

        protected string Name { get; set; }
        internal DateTime Created { get; set; }
        public decimal UpdatedProp { get; set; }
    }

    public class NoSetterProperty
    {
        protected string Name { get; }
    }
}
