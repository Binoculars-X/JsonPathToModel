using JsonPathToModel.Parser;
using JsonPathToModel.Tests.Examples;
using JsonPathToModel.Tests.ModelData;
using Sigil;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JsonPathToModel.Tests.Parser;

public class CollectionEmitterPrototypeTests
{
    private static BindingFlags _visibilityAll = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public;

    [Fact]
    public void Prototype_ArrayAccess_ShouldWork_WithSigilEmitter()
    {
        // Arrange
        var model = SampleClientModelTests.GenerateSampleClient();
        // Clear and add specific test data
        model.Person.Emails.Clear();
        model.Person.Emails.Add(new Email { Value = "test1@email.com" });
        model.Person.Emails.Add(new Email { Value = "test2@email.com" });
        
        // Expected value from direct access
        var expected = model.Person.Emails[0].Value;

        // Act - Create IL emitter for "$.Person.Emails[0].Value" pattern
        var emittedDelegate = CreateArrayAccessEmitter();
        var result = emittedDelegate(model);

        // Assert
        Assert.Equal(expected, result);
        Assert.Equal("test1@email.com", result);
    }

    [Fact]
    public void Prototype_ArrayAccess_ShouldWork_WithRoleArray()
    {
        // Arrange
        var model = SampleClientModelTests.GenerateSampleClient();
        
        // Override with specific test data
        model.Roles = new[] { new Role { RoleId = "1", Name = "Admin" } };
        
        // Expected value from direct access
        var expected = model.Roles[0].Name;

        // Act - Create IL emitter for "$.Roles[0].Name" pattern
        var emittedDelegate = CreateRoleArrayAccessEmitter();
        var result = emittedDelegate(model);

        // Assert
        Assert.Equal(expected, result);
        Assert.Equal("Admin", result);
    }

    [Fact]
    public void Prototype_StringArrayAccess_ShouldWork()
    {
        // Arrange - Create a model with string array
        var model = SampleClientModelTests.GenerateSampleClient();
        model.Person.Addresses.Clear();
        model.Person.Addresses.Add(new Address 
        { 
            AddressLine = new[] { "123 Main St", "Apt 4B", "Building C" }
        });
        
        // Expected value from direct access
        var expected = model.Person.Addresses[0].AddressLine[1]; // "Apt 4B"

        // Act - Create IL emitter for "$.Person.Addresses[0].AddressLine[1]" pattern
        var emittedDelegate = CreateStringArrayAccessEmitter();
        var result = emittedDelegate(model);

        // Assert
        Assert.Equal(expected, result);
        Assert.Equal("Apt 4B", result);
    }

    /// <summary>
    /// Creates an emitter that mimics: model.Person.Emails[0].Value
    /// Demonstrates List&lt;T&gt; access pattern
    /// </summary>
    private static Func<object, object> CreateArrayAccessEmitter()
    {
        // Create Sigil emitter - equivalent to: (object model) => ((SampleClientModel)model).Person.Emails[0].Value
        var result = Emit<Func<object, object>>.NewDynamicMethod();
        
        // Load argument 0 (the model object)
        result.LoadArgument(0);
        
        // Cast to SampleClientModel
        result.CastClass(typeof(SampleClientModel));
        
        // Call get_Person() property
        var personProperty = typeof(SampleClientModel).GetProperty("Person", _visibilityAll)!;
        result.Call(personProperty.GetGetMethod(true)!);
        
        // Call get_Emails() property
        var emailsProperty = typeof(Person).GetProperty("Emails", _visibilityAll)!;
        result.Call(emailsProperty.GetGetMethod(true)!);
        
        // Cast to IList for indexer access
        result.CastClass(typeof(IList));
        
        // Load constant 0 (array index)
        result.LoadConstant(0);
        
        // Call IList.get_Item(int index) - this is the indexer
        var getItemMethod = typeof(IList).GetMethod("get_Item")!;
        result.CallVirtual(getItemMethod);
        
        // Cast result to Email type
        result.CastClass(typeof(Email));
        
        // Call get_Value() property
        var valueProperty = typeof(Email).GetProperty("Value", _visibilityAll)!;
        result.Call(valueProperty.GetGetMethod(true)!);
        
        // Box the string result (string is reference type but we want object)
        // Actually, string doesn't need boxing, but let's be consistent with the pattern
        result.Return();
        
        // Create the delegate
        return result.CreateDelegate();
    }

    /// <summary>
    /// Creates an emitter that mimics: model.Roles[0].Name
    /// Demonstrates Array access pattern
    /// </summary>
    private static Func<object, object> CreateRoleArrayAccessEmitter()
    {
        var result = Emit<Func<object, object>>.NewDynamicMethod();
        
        // Load argument 0 (the model object)
        result.LoadArgument(0);
        
        // Cast to SampleClientModel
        result.CastClass(typeof(SampleClientModel));
        
        // Call get_Roles() property - returns Role[]
        var rolesProperty = typeof(SampleClientModel).GetProperty("Roles", _visibilityAll)!;
        result.Call(rolesProperty.GetGetMethod(true)!);
        
        // Load constant 0 (array index)
        result.LoadConstant(0);
        
        // Use LoadElement for array access (more efficient than IList for arrays)
        result.LoadElement(typeof(Role));
        
        // Call get_Name() property
        var nameProperty = typeof(Role).GetProperty("Name", _visibilityAll)!;
        result.Call(nameProperty.GetGetMethod(true)!);
        
        result.Return();
        
        return result.CreateDelegate();
    }

    /// <summary>
    /// Creates an emitter that mimics: model.Person.Addresses[0].AddressLine[1]
    /// Demonstrates nested array access pattern
    /// </summary>
    private static Func<object, object> CreateStringArrayAccessEmitter()
    {
        var result = Emit<Func<object, object>>.NewDynamicMethod();
        
        // Load argument 0 (the model object)
        result.LoadArgument(0);
        
        // Cast to SampleClientModel
        result.CastClass(typeof(SampleClientModel));
        
        // Call get_Person() property
        var personProperty = typeof(SampleClientModel).GetProperty("Person", _visibilityAll)!;
        result.Call(personProperty.GetGetMethod(true)!);
        
        // Call get_Addresses() property (List<Address>)
        var addressesProperty = typeof(Person).GetProperty("Addresses", _visibilityAll)!;
        result.Call(addressesProperty.GetGetMethod(true)!);
        
        // Cast to IList and access [0]
        result.CastClass(typeof(IList));
        result.LoadConstant(0);
        result.CallVirtual(typeof(IList).GetMethod("get_Item")!);
        
        // Cast to Address
        result.CastClass(typeof(Address));
        
        // Call get_AddressLine() property (string[])
        var addressLineProperty = typeof(Address).GetProperty("AddressLine", _visibilityAll)!;
        result.Call(addressLineProperty.GetGetMethod(true)!);
        
        // Load constant 1 (second element of string array)
        result.LoadConstant(1);
        
        // Use LoadElement for string array access
        result.LoadElement(typeof(string));
        
        result.Return();
        
        return result.CreateDelegate();
    }

    /// <summary>
    /// Demonstrates a generalized collection access method that could be integrated
    /// into the ExpressionEngine.GetStraightEmitterGet method
    /// </summary>
    [Fact]
    public void Prototype_GeneralizedCollectionAccess_ShouldWork()
    {
        // Arrange
        var model = SampleClientModelTests.GenerateSampleClient();
        model.Person.Emails.Clear();
        model.Person.Emails.Add(new Email { Value = "generalized@test.com" });
        
        // Expected
        var expected = model.Person.Emails[0].Value;
        
        // Act - Use generalized collection access emitter
        var emittedDelegate = CreateGeneralizedCollectionEmitter(
            typeof(SampleClientModel),
            new[]
            {
                ("Person", (int?)null),              // Property access
                ("Emails", (int?)0),                 // Collection access at index 0
                ("Value", (int?)null)                // Property access
            });
        
        var result = emittedDelegate(model);
        
        // Assert
        Assert.Equal(expected, result);
        Assert.Equal("generalized@test.com", result);
    }

    /// <summary>
    /// Creates a generalized collection access emitter based on a path descriptor
    /// This demonstrates how the pattern could be integrated into ExpressionEngine
    /// </summary>
    private static Func<object, object> CreateGeneralizedCollectionEmitter(
        Type modelType, 
        (string PropertyName, int? CollectionIndex)[] path)
    {
        var result = Emit<Func<object, object>>.NewDynamicMethod();
        
        // Load argument 0 (the model object)
        result.LoadArgument(0);
        
        Type currentType = modelType;
        
        foreach (var (propertyName, collectionIndex) in path)
        {
            // Cast to current type
            result.CastClass(currentType);
            
            // Get property info
            var property = currentType.GetProperty(propertyName, _visibilityAll)!;
            
            // Call property getter
            result.Call(property.GetGetMethod(true)!);
            
            // If this is a collection access
            if (collectionIndex.HasValue)
            {
                // Determine if it's an array or IList
                if (property.PropertyType.IsArray)
                {
                    // Array access using LoadElement
                    result.LoadConstant(collectionIndex.Value);
                    result.LoadElement(property.PropertyType.GetElementType()!);
                    currentType = property.PropertyType.GetElementType()!;
                }
                else if (typeof(IList).IsAssignableFrom(property.PropertyType))
                {
                    // IList access using get_Item
                    result.CastClass(typeof(IList));
                    result.LoadConstant(collectionIndex.Value);
                    result.CallVirtual(typeof(IList).GetMethod("get_Item")!);
                    
                    // Determine element type for generic collections
                    if (property.PropertyType.IsGenericType)
                    {
                        var genericArgs = property.PropertyType.GetGenericArguments();
                        currentType = genericArgs[0]; // For List<T>, this is T
                        result.CastClass(currentType);
                    }
                    else
                    {
                        currentType = typeof(object); // Non-generic collections
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Property {propertyName} is not a supported collection type");
                }
            }
            else
            {
                // Regular property access - update current type
                currentType = property.PropertyType;
            }
        }
        
        result.Return();
        return result.CreateDelegate();
    }

    /// <summary>
    /// Performance comparison test to demonstrate the speed difference
    /// between IL emitted collection access and direct C# access
    /// </summary>
    [Fact]
    public void Performance_Comparison_EmittedVsDirect()
    {
        // Arrange
        var model = SampleClientModelTests.GenerateSampleClient();
        model.Person.Emails.Clear();
        model.Person.Emails.Add(new Email { Value = "test@email.com" });
        
        var emittedDelegate = CreateArrayAccessEmitter();
        
        const int iterations = 1_000_000;
        
        // Warm up
        for (int i = 0; i < 1000; i++)
        {
            var _ = model.Person.Emails[0].Value;
            var __ = emittedDelegate(model);
        }
        
        // Test direct access
        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var result = model.Person.Emails[0].Value;
        }
        sw.Stop();
        var directTime = sw.ElapsedMilliseconds;
        
        // Test emitted access
        sw.Restart();
        for (int i = 0; i < iterations; i++)
        {
            var result = emittedDelegate(model);
        }
        sw.Stop();
        var emittedTime = sw.ElapsedMilliseconds;
        
        // Output for analysis
        var directNsPerOp = (directTime * 1_000_000.0) / iterations;
        var emittedNsPerOp = (emittedTime * 1_000_000.0) / iterations;
        var slowdown = emittedNsPerOp / directNsPerOp;
        
        // Assert that emitted access is reasonably fast (less than 10x slower than direct)
        Assert.True(slowdown < 10, 
            $"Emitted access too slow: Direct={directNsPerOp:F1}ns, Emitted={emittedNsPerOp:F1}ns, Slowdown={slowdown:F1}x");
            
        // Also verify correctness
        Assert.Equal(model.Person.Emails[0].Value, emittedDelegate(model));
    }
}
