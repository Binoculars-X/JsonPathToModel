using JsonPathToModel.Tests.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests.Navigation;

public class NonPublicGetSetValueTests
{
    [Fact]
    public void Should_Get_NonPublicProperties()
    {
        var test = (IJsonPathModelNavigator navi) =>
        {
            var model = new NonPublicProperties("123", true, 124);
            Assert.Equal("123", navi.GetValue(model, "$.Id"));
            Assert.Equal(true, navi.GetValue(model, "$.Bool"));
            Assert.Equal(124, navi.GetValue(model, "$.Number"));
        };
            
        // use reflection
        var svc = ConfigHelper.GetConfigurationServices();
        var navi = svc.GetService<IJsonPathModelNavigator>()!;
        test(navi);

        // use optimisation
        svc = ConfigHelper.GetConfigurationServices(true);
        navi = svc.GetService<IJsonPathModelNavigator>()!;
        test(navi);
    }

    [Fact]
    public void Should_Set_NonPublicProperties()
    {
        var test = (IJsonPathModelNavigator navi) =>
        {
            var model = new NonPublicProperties();
            navi.SetValue(model, "$.Id", "123");
            navi.SetValue(model, "$.Bool", true);
            navi.SetValue(model, "$.Number", 77);

            Assert.Equal("123", model.AcccessorId);
            Assert.True(model.AcccessorBool);
            Assert.Equal(77, model.AcccessorNumber);
        };

        // use reflection
        var svc = ConfigHelper.GetConfigurationServices();
        var navi = svc.GetService<IJsonPathModelNavigator>()!;
        test(navi);

        // use optimisation
        svc = ConfigHelper.GetConfigurationServices(true);
        navi = svc.GetService<IJsonPathModelNavigator>()!;
        test(navi);
    }

    [Fact]
    public void Should_Get_NonPublicFields()
    {
        var test = (IJsonPathModelNavigator navi) =>
        {
            var model = new NonPublicFields("123", true, 124);
            Assert.Equal("123", navi.GetValue(model, "$.Id"));
            Assert.Equal(true, navi.GetValue(model, "$.Bool"));
            Assert.Equal(124, navi.GetValue(model, "$.Number"));
        };

        // use reflection
        var svc = ConfigHelper.GetConfigurationServices();
        var navi = svc.GetService<IJsonPathModelNavigator>()!;
        test(navi);

        // use optimisation
        svc = ConfigHelper.GetConfigurationServices(true);
        navi = svc.GetService<IJsonPathModelNavigator>()!;
        test(navi);
    }

    [Fact]
    public void Should_Set_NonPublicFields()
    {
        var test = (IJsonPathModelNavigator navi) =>
        {
            var model = new NonPublicFields();
            navi.SetValue(model, "$.Id", "123");
            navi.SetValue(model, "$.Bool", true);
            navi.SetValue(model, "$.Number", 77);

            Assert.Equal("123", model.AcccessorId);
            Assert.True(model.AcccessorBool);
            Assert.Equal(77, model.AcccessorNumber);
        };

        // use reflection
        var svc = ConfigHelper.GetConfigurationServices();
        var navi = svc.GetService<IJsonPathModelNavigator>()!;
        test(navi);

        // use optimisation
        svc = ConfigHelper.GetConfigurationServices(true);
        navi = svc.GetService<IJsonPathModelNavigator>()!;
        test(navi);
    }

    public class NonPublicProperties
    {
        public NonPublicProperties() 
        { 
        }

        public NonPublicProperties(string id, bool boolProp, int number)
        {
            Id = id;
            Bool = boolProp;
            Number = number;
        }

        private string? Id { get; set; }
        internal bool Bool { get; set; }
        protected int Number { get; set; }

        public string AcccessorId { get { return Id; } }
        public bool AcccessorBool { get { return Bool; } }
        public int AcccessorNumber { get { return Number; } }
    }

    public class NonPublicFields
    {
        public NonPublicFields() 
        {
        }

        public NonPublicFields(string id, bool boolProp, int number)
        {
            Id = id;
            Bool = boolProp;
            Number = number;
        }

        private string? Id;
        internal bool Bool;
        protected int Number;

        public string AcccessorId { get { return Id; } }
        public bool AcccessorBool { get { return Bool; } }
        public int AcccessorNumber { get { return Number; } }
    }
}
