# Collection Emitters Implementation Plan

## Overview
This document outlines the implementation plan to add collection support to the IL emitters in the JsonPathToModel library. Currently, the `GetStraightEmitterGet()` and `GetStraightEmitterSetDetails()` methods bail out when collections are detected, forcing fallback to slower reflection-based access.

## Current State Analysis

### What Works Now
- ✅ Simple property chains: `$.Person.FirstName` 
- ✅ Nested objects: `$.Customer.Person.Address.Street`
- ✅ Performance: ~25-28ns vs ~124ns without optimization

### What's Missing
- ❌ Array access: `$.Person.Emails[0].Value`
- ❌ Dictionary access: `$.Settings['theme'].Value`
- ❌ Wildcard collections: `$.Person.Emails[*].Value`

### Current Bottlenecks
```csharp
// In ExpressionEngine.cs - both methods have this check:
if (!_options.OptimizeWithCodeEmitter || tokens.Any(t => t.CollectionDetails != null))
{
    // collections not supported yet
    return null;
}
```

## Technical Architecture

### Collection Types to Support
1. **IList/Array** - Integer indexing: `[0]`, `[1]`, etc.
2. **IDictionary** - String key access: `['key']`, `["key"]`
3. **Wildcard Access** - All items: `[*]`, `[]`

### Sigil IL Emission Strategy

#### For IList/Array Access Pattern:
```csharp
// Target: obj.Collection[index].Property
// IL equivalent to: ((IList)obj.Collection)[index]

result.LoadArgument(0);                    // Load target object
result.CastClass(currentType);             // Cast to correct type
result.Call(collectionProp.GetGetMethod()); // Call Collection getter
result.CastClass(typeof(IList));           // Cast to IList
result.LoadConstant(index);                // Load array index
result.CallVirtual(typeof(IList).GetMethod("get_Item")); // Call indexer
```

#### For IDictionary Access Pattern:
```csharp
// Target: obj.Dictionary["key"].Property  
// IL equivalent to: ((IDictionary)obj.Dictionary)["key"]

result.LoadArgument(0);                    // Load target object
result.CastClass(currentType);             // Cast to correct type  
result.Call(dictionaryProp.GetGetMethod()); // Call Dictionary getter
result.CastClass(typeof(IDictionary));     // Cast to IDictionary
result.LoadConstant(key);                  // Load dictionary key
result.CallVirtual(typeof(IDictionary).GetMethod("get_Item")); // Call indexer
```

## Implementation Plan

### Phase 1: Core Infrastructure (Week 1)

#### 1.1 Extend TokenInfo Analysis
**File:** `Parser/ExpressionEngine.cs`

Add method to detect collection compatibility:
```csharp
private bool CanOptimizeWithCollections(List<TokenInfo> tokens)
{
    // Only support single index access initially - no wildcards
    return tokens.Where(t => t.CollectionDetails != null)
                 .All(t => !t.CollectionDetails.SelectAll && 
                          (t.CollectionDetails.Index.HasValue || 
                           !string.IsNullOrEmpty(t.CollectionDetails.Literal)));
}
```

#### 1.2 Update Optimization Condition
Replace current bail-out logic:
```csharp
// OLD:
if (!_options.OptimizeWithCodeEmitter || tokens.Any(t => t.CollectionDetails != null))

// NEW:
if (!_options.OptimizeWithCodeEmitter || !CanOptimizeWithCollections(tokens))
```

### Phase 2: IList/Array Support (Week 1-2)

#### 2.1 Implement GetStraightEmitterGet for Arrays
**File:** `Parser/ExpressionEngine.cs`

```csharp
private Emit<Func<object, object>>? GetStraightEmitterGet(string expression, Type modelType, List<TokenInfo> tokens)
{
    if (!_options.OptimizeWithCodeEmitter || !CanOptimizeWithCollections(tokens))
    {
        return null;
    }

    Emit<Func<object, object>> result;
    var currentType = modelType;
    result = Emit<Func<object, object>>.NewDynamicMethod();
    result.LoadArgument(0);

    foreach (var token in tokens)
    {
        if (token.Token == Token.Dollar)
        {
            continue;
        }

        var propInfo = currentType.GetProperty(token.Field, _visibilityAll);
        
        if (propInfo == null)
        {
            // Handle field access (TODO: implement if needed)
            return null;
        }

        result.CastClass(currentType);
        result.Call(propInfo.GetGetMethod(true)!);

        // Handle collection access
        if (token.CollectionDetails != null)
        {
            if (!EmitCollectionAccess(result, token, propInfo.PropertyType))
            {
                return null;
            }
            
            // Determine the element type for next iteration
            currentType = GetCollectionElementType(propInfo.PropertyType);
        }
        else
        {
            // Regular property access
            using (var a = result.DeclareLocal(propInfo.PropertyType))
            {
                result.StoreLocal(a);
                result.LoadLocal(a);
            }
            
            currentType = propInfo.PropertyType;
        }
    }

    if (currentType.IsBoxable())
    {
        result.Box(currentType);
    }

    result.Return();
    return result;
}
```

#### 2.2 Implement Collection Access Helper
```csharp
private bool EmitCollectionAccess(Emit<Func<object, object>> result, TokenInfo token, Type collectionType)
{
    if (token.CollectionDetails.Index.HasValue)
    {
        // Array/IList access: collection[index]
        if (collectionType.IsArray)
        {
            result.LoadConstant(token.CollectionDetails.Index.Value);
            result.LoadElement(collectionType.GetElementType()!);
        }
        else if (typeof(IList).IsAssignableFrom(collectionType))
        {
            result.CastClass(typeof(IList));
            result.LoadConstant(token.CollectionDetails.Index.Value);
            result.CallVirtual(typeof(IList).GetMethod("get_Item")!);
        }
        else
        {
            return false; // Unsupported collection type
        }
    }
    else if (!string.IsNullOrEmpty(token.CollectionDetails.Literal))
    {
        // Dictionary access: dictionary["key"]
        if (typeof(IDictionary).IsAssignableFrom(collectionType))
        {
            result.CastClass(typeof(IDictionary));
            result.LoadConstant(token.CollectionDetails.Literal);
            result.CallVirtual(typeof(IDictionary).GetMethod("get_Item")!);
        }
        else
        {
            return false; // Unsupported collection type
        }
    }
    else
    {
        return false; // Wildcard access not supported yet
    }

    return true;
}
```

#### 2.3 Add Element Type Detection
```csharp
private Type GetCollectionElementType(Type collectionType)
{
    if (collectionType.IsArray)
    {
        return collectionType.GetElementType()!;
    }
    
    if (collectionType.IsGenericType)
    {
        var genericArgs = collectionType.GetGenericArguments();
        
        // IList<T>, ICollection<T>, IEnumerable<T>
        if (typeof(IEnumerable).IsAssignableFrom(collectionType) && genericArgs.Length == 1)
        {
            return genericArgs[0];
        }
        
        // IDictionary<K,V> - return value type
        if (typeof(IDictionary).IsAssignableFrom(collectionType) && genericArgs.Length == 2)
        {
            return genericArgs[1];
        }
    }
    
    // Non-generic collections return object
    return typeof(object);
}
```

### Phase 3: Setter Support (Week 2)

#### 3.1 Implement GetStraightEmitterSetDetails for Collections
**File:** `Parser/ExpressionEngine.cs`

Similar pattern but for setters:
```csharp
private SetDelegateDetails? GetStraightEmitterSetDetails(string expression, Type modelType, List<TokenInfo> tokens)
{
    if (!_options.OptimizeWithCodeEmitter || !CanOptimizeWithCollections(tokens))
    {
        return null;
    }

    var result = Emit<Action<object, object>>.NewDynamicMethod();
    var currentType = modelType;
    result.LoadArgument(0);

    // Navigate to parent of target property
    for (int i = 1; i < tokens.Count - 1; i++)
    {
        var token = tokens[i];
        var propInfo = currentType.GetProperty(token.Field, _visibilityAll);
        
        if (propInfo == null) return null;
        
        result.CastClass(currentType);
        result.Call(propInfo.GetGetMethod(true)!);
        
        if (token.CollectionDetails != null)
        {
            if (!EmitCollectionAccessForSetter(result, token, propInfo.PropertyType))
            {
                return null;
            }
            currentType = GetCollectionElementType(propInfo.PropertyType);
        }
        else
        {
            using (var a = result.DeclareLocal(propInfo.PropertyType))
            {
                result.StoreLocal(a);
                result.LoadLocal(a);
            }
            currentType = propInfo.PropertyType;
        }
    }

    // Handle final property assignment
    var lastToken = tokens.Last();
    if (lastToken.CollectionDetails != null)
    {
        return EmitCollectionSetter(result, lastToken, currentType, expression);
    }
    else
    {
        return EmitPropertySetter(result, lastToken, currentType, expression);
    }
}
```

### Phase 4: Error Handling & Edge Cases (Week 2-3)

#### 4.1 Null Reference Protection
```csharp
private void EmitNullCheck(Emit<Func<object, object>> result, Label nullLabel)
{
    result.Duplicate();
    result.LoadNull();
    result.BranchIfEqual(nullLabel);
}
```

#### 4.2 Bounds Checking for Arrays
```csharp
private void EmitBoundsCheck(Emit<Func<object, object>> result, int index)
{
    // Duplicate array reference for length check
    result.Duplicate();
    result.LoadLength();
    result.LoadConstant(index);
    // Branch if index >= length
    var boundsOkLabel = result.DefineLabel();
    result.BranchIfGreaterThanOrEqual(boundsOkLabel);
    // Throw IndexOutOfRangeException
    result.NewObject(typeof(IndexOutOfRangeException).GetConstructor(Type.EmptyTypes)!);
    result.Throw();
    result.MarkLabel(boundsOkLabel);
}
```

### Phase 5: Testing & Validation (Week 3)

#### 5.1 Unit Tests
**File:** `tests/JsonPathToModel.Tests/Parser/CollectionEmitterTests.cs`

```csharp
public class CollectionEmitterTests
{
    [Fact] 
    public void EmittedGet_Should_HandleArrayAccess()
    {
        // Test: $.Emails[0].Value
    }
    
    [Fact]
    public void EmittedGet_Should_HandleDictionaryAccess() 
    {
        // Test: $.Settings["theme"].Value
    }
    
    [Fact]
    public void EmittedSet_Should_HandleArrayAssignment()
    {
        // Test: $.Emails[0].Value = "new@email.com"
    }
    
    [Fact]
    public void EmittedAccess_Should_HandleNullCollections()
    {
        // Test null safety
    }
}
```

#### 5.2 Performance Benchmarks
**File:** `tests/BenchmarkConsoleApp/CollectionBenchmarks.cs`

```csharp
[MemoryDiagnoser]
public class CollectionBenchmarks
{
    [Benchmark] public string DirectArrayAccess() => model.Emails[0].Value;
    [Benchmark] public string EmittedArrayAccess() => navigator.GetValue(model, "$.Emails[0].Value");
    [Benchmark] public string ReflectionArrayAccess() => navigatorSlow.GetValue(model, "$.Emails[0].Value");
}
```

## Expected Performance Improvements

### Current Performance (Collections via Reflection)
- Array access: `$.Emails[0].Value` → ~155ns
- Dictionary access: `$.Settings['key'].Value` → ~180ns

### Target Performance (Collections via IL)
- Array access: `$.Emails[0].Value` → ~35-45ns (4x improvement)  
- Dictionary access: `$.Settings['key'].Value` → ~40-50ns (3.5x improvement)

## Implementation Phases Timeline

- **Week 1:** Infrastructure + IList/Array support  
- **Week 2:** Setter support + IDictionary support
- **Week 3:** Error handling + Testing + Documentation
- **Week 4:** Wildcard support (stretch goal)

## Future Enhancements (Post-MVP)

### Wildcard Support
- `$.Emails[*].Value` - Return all email values
- Requires different return signatures (IEnumerable)
- Complex IL generation for iteration

### Generic Collections
- `List<T>`, `Dictionary<K,V>` optimizations
- Avoid boxing/unboxing where possible
- Type-specific IL generation

### Advanced Indexing  
- `$.Matrix[1,2]` - Multi-dimensional arrays
- `$.Items[^1]` - Index from end (C# 8+)

## Risk Mitigation

### Fallback Strategy
Always maintain current reflection-based approach as fallback:
```csharp
// If IL generation fails, fall back to reflection
if (optimizedDelegate == null)
{
    return ReflectionBasedAccess(tokens, target);
}
```

### Gradual Rollout
- Start with simple cases (single array/dictionary access)
- Add complexity incrementally  
- Extensive test coverage at each phase

## Conclusion

This implementation plan provides a structured approach to adding collection support to the IL emitters, with clear phases, measurable performance targets, and risk mitigation strategies. The expected 3-4x performance improvement for collection access will significantly benefit applications doing heavy JSON path operations.
