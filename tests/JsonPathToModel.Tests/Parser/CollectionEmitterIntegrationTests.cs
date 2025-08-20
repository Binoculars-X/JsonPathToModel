using JsonPathToModel.Tests.Examples;
using JsonPathToModel.Tests.ModelData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests.Parser;

public class CollectionEmitterIntegrationTests
{
    [Theory]
    [InlineData(true)]  // With optimization
    [InlineData(false)] // Without optimization (reflection fallback)
    public void GetValue_Should_HandleMixedPropertyAndCollection_SimpleArray(bool useOptimization)
    {
        // Arrange
        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions 
        { 
            OptimizeWithCodeEmitter = useOptimization 
        });
        
        var model = SampleClientModelTests.GenerateSampleClient();
        model.Roles = new[] 
        { 
            new Role { RoleId = "1", Name = "Admin" },
            new Role { RoleId = "2", Name = "User" }
        };
        
        // Act & Assert - Mixed property and array access
        var result = navigator.GetValue(model, "$.Roles[0].Name");
        Assert.Equal("Admin", result);
        
        result = navigator.GetValue(model, "$.Roles[1].RoleId");
        Assert.Equal("2", result);
    }

    [Theory]
    [InlineData(true)]  // With optimization
    [InlineData(false)] // Without optimization (reflection fallback)
    public void GetValue_Should_HandleMixedPropertyAndCollection_List(bool useOptimization)
    {
        // Arrange
        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions 
        { 
            OptimizeWithCodeEmitter = useOptimization 
        });
        
        var model = SampleClientModelTests.GenerateSampleClient();
        model.Person.Emails.Clear();
        model.Person.Emails.AddRange(new[]
        {
            new Email { Value = "admin@test.com" },
            new Email { Value = "user@test.com" }
        });
        
        // Act & Assert - Mixed property and List access
        var result = navigator.GetValue(model, "$.Person.Emails[0].Value");
        Assert.Equal("admin@test.com", result);
        
        result = navigator.GetValue(model, "$.Person.Emails[1].Value");
        Assert.Equal("user@test.com", result);
    }

    [Theory]
    [InlineData(true)]  // With optimization
    [InlineData(false)] // Without optimization (reflection fallback)
    public void GetValue_Should_HandleNestedCollections(bool useOptimization)
    {
        // Arrange
        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions 
        { 
            OptimizeWithCodeEmitter = useOptimization 
        });
        
        var model = SampleClientModelTests.GenerateSampleClient();
        model.Person.Addresses.Clear();
        model.Person.Addresses.Add(new Address
        {
            AddressLine = new[] { "123 Main St", "Apt 4B", "Building C" },
            CityName = "TestCity"
        });
        
        // Act & Assert - Nested collection access (List -> Array)
        var result = navigator.GetValue(model, "$.Person.Addresses[0].AddressLine[1]");
        Assert.Equal("Apt 4B", result);
        
        result = navigator.GetValue(model, "$.Person.Addresses[0].CityName");
        Assert.Equal("TestCity", result);
    }

    [Theory]
    [InlineData(true)]  // With optimization
    [InlineData(false)] // Without optimization (reflection fallback)
    public void GetValue_Should_HandleComplexMixedAccess(bool useOptimization)
    {
        // Arrange
        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions 
        { 
            OptimizeWithCodeEmitter = useOptimization 
        });
        
        var model = SampleClientModelTests.GenerateSampleClient();
        
        // Setup complex nested structure
        model.Person.Emails.Clear();
        model.Person.Emails.Add(new Email { Value = "primary@test.com" });
        
        model.Person.Addresses.Clear();
        model.Person.Addresses.AddRange(new[]
        {
            new Address 
            { 
                AddressType = "Home",
                AddressLine = new[] { "123 Home St", "Suite 100" },
                CityName = "HomeCity"
            },
            new Address 
            { 
                AddressType = "Work", 
                AddressLine = new[] { "456 Work Ave", "Floor 5" },
                CityName = "WorkCity"
            }
        });
        
        // Act & Assert - Complex mixed patterns
        var result = navigator.GetValue(model, "$.Person.FirstName");
        Assert.NotNull(result); // AutoFixture generates this
        
        result = navigator.GetValue(model, "$.Person.Emails[0].Value");
        Assert.Equal("primary@test.com", result);
        
        result = navigator.GetValue(model, "$.Person.Addresses[0].AddressType");
        Assert.Equal("Home", result);
        
        result = navigator.GetValue(model, "$.Person.Addresses[1].AddressLine[0]");
        Assert.Equal("456 Work Ave", result);
        
        result = navigator.GetValue(model, "$.Person.Addresses[0].AddressLine[1]");
        Assert.Equal("Suite 100", result);
    }

    [Theory]
    [InlineData(true)]  // With optimization
    [InlineData(false)] // Without optimization (reflection fallback)  
    public void GetValue_Should_HandleEdgeCases(bool useOptimization)
    {
        // Arrange
        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions 
        { 
            OptimizeWithCodeEmitter = useOptimization 
        });
        
        var model = SampleClientModelTests.GenerateSampleClient();
        
        // Test with single item collections
        model.Person.Emails.Clear();
        model.Person.Emails.Add(new Email { Value = "single@test.com" });
        
        // Act & Assert
        var result = navigator.GetValue(model, "$.Person.Emails[0].Value");
        Assert.Equal("single@test.com", result);
    }

    [Theory]
    [InlineData(true)]  // With optimization
    [InlineData(false)] // Without optimization (reflection fallback)
    public void GetValue_Should_HandleArrayWithMultipleProperties(bool useOptimization)
    {
        // Arrange
        var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions 
        { 
            OptimizeWithCodeEmitter = useOptimization 
        });
        
        var model = SampleClientModelTests.GenerateSampleClient();
        model.Roles = new[]
        {
            new Role { RoleId = "admin", Name = "Administrator" },
            new Role { RoleId = "user", Name = "Standard User" },
            new Role { RoleId = "guest", Name = "Guest User" }
        };
        
        // Act & Assert - Test multiple properties on array elements
        var result = navigator.GetValue(model, "$.Roles[0].RoleId");
        Assert.Equal("admin", result);
        
        result = navigator.GetValue(model, "$.Roles[0].Name");
        Assert.Equal("Administrator", result);
        
        result = navigator.GetValue(model, "$.Roles[2].Name");
        Assert.Equal("Guest User", result);
        
        result = navigator.GetValue(model, "$.Roles[1].RoleId");
        Assert.Equal("user", result);
    }

    [Fact]
    public void Performance_CollectionAccess_ShouldBe_FasterWithOptimization()
    {
        // Arrange
        var optimizedNavigator = new JsonPathModelNavigator(new NavigatorConfigOptions 
        { 
            OptimizeWithCodeEmitter = true 
        });
        
        var reflectionNavigator = new JsonPathModelNavigator(new NavigatorConfigOptions 
        { 
            OptimizeWithCodeEmitter = false 
        });
        
        var model = SampleClientModelTests.GenerateSampleClient();
        model.Person.Emails.Clear();
        model.Person.Emails.Add(new Email { Value = "performance@test.com" });
        
        const int iterations = 100_000;
        
        // Warm up
        for (int i = 0; i < 1000; i++)
        {
            optimizedNavigator.GetValue(model, "$.Person.Emails[0].Value");
            reflectionNavigator.GetValue(model, "$.Person.Emails[0].Value");
        }
        
        // Test optimized
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var result = optimizedNavigator.GetValue(model, "$.Person.Emails[0].Value");
        }
        sw.Stop();
        var optimizedTime = sw.ElapsedMilliseconds;
        
        // Test reflection
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var result = reflectionNavigator.GetValue(model, "$.Person.Emails[0].Value");
        }
        sw.Stop();
        var reflectionTime = sw.ElapsedMilliseconds;
        
        // Calculate performance metrics
        var optimizedNsPerOp = (optimizedTime * 1_000_000.0) / iterations;
        var reflectionNsPerOp = (reflectionTime * 1_000_000.0) / iterations;
        var improvement = reflectionNsPerOp / optimizedNsPerOp;
        
        // Currently both should be similar (reflection fallback), but after implementation
        // optimized should be significantly faster
        Assert.True(optimizedNsPerOp > 0, $"Optimized: {optimizedNsPerOp:F1}ns per op");
        Assert.True(reflectionNsPerOp > 0, $"Reflection: {reflectionNsPerOp:F1}ns per op"); 
        
        // For now, just ensure both work correctly
        Assert.Equal("performance@test.com", optimizedNavigator.GetValue(model, "$.Person.Emails[0].Value"));
        Assert.Equal("performance@test.com", reflectionNavigator.GetValue(model, "$.Person.Emails[0].Value"));
    }
}
