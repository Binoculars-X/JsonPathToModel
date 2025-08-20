# Bug Report: Collection Element Setting Not Supported

## Issue Summary
The current JsonPathToModel library does not support setting individual elements within collections (arrays, lists, dictionaries). Attempts to set collection elements result in `ArgumentException` with the message "Object of type 'System.String' cannot be converted to type 'System.String[]'".

## Environment
- **Library Version**: Current development branch `feature/issue-12-sigil-collections-support`
- **Target Framework**: .NET 9.0
- **Date Reported**: August 20, 2025

## Problem Description
When using JsonPath expressions to set values on collection elements (e.g., `$.MiddleNames[0]`, `$.Users[1].Name`), the library incorrectly attempts to set the entire collection property instead of the specific collection element.

### Root Cause
The current setter implementation in `ExpressionResultExtensions.SetValue()` does not handle collection indexing. When a path contains collection details (e.g., `[0]`, `["key"]`), the library treats the collection access as a regular property access, leading to type conversion errors.

## Reproduction Steps

### Test Case 1: Simple Array Element Setting
```csharp
var model = new SampleModel
{
    MiddleNames = ["First", "Second", "Third"]
};

var navigator = new JsonPathModelNavigator();
navigator.SetValue(model, "$.MiddleNames[1]", "Updated"); // FAILS
```

**Expected**: `model.MiddleNames[1]` should be set to `"Updated"`
**Actual**: Throws `ArgumentException: Object of type 'System.String' cannot be converted to type 'System.String[]'`

### Test Case 2: List Element Property Setting
```csharp
var model = new SampleModel
{
    NestedList = [
        new SampleNested { Id = "1", Name = "First" }
    ]
};

var navigator = new JsonPathModelNavigator();
navigator.SetValue(model, "$.NestedList[0].Name", "Updated"); // WORKS
```

**Status**: ✅ This works because it sets a property on an object within a collection, not the collection element itself.

### Test Case 3: Collection Element Setting with Configuration
```csharp
var navigator = new JsonPathModelNavigator(new NavigatorConfigOptions
{
    OptimizeWithCodeEmitter = false, // Even with reflection fallback
    FailOnCollectionKeyNotFound = false
});

navigator.SetValue(model, "$.MiddleNames[0]", "Test"); // FAILS
```

**Status**: ❌ Fails regardless of optimization settings

## Error Details

### Stack Trace
```
System.ArgumentException: Object of type 'System.String' cannot be converted to type 'System.String[]'.
   at System.RuntimeType.CheckValue(Object& value, Binder binder, CultureInfo culture, BindingFlags invokeAttr)
   at System.Reflection.RuntimePropertyInfo.SetValue(Object obj, Object value, Object[] index)
   at JsonPathToModel.ExpressionResultExtensions.SetValue(ExpressionResult result, Object target, Object val, NavigatorConfigOptions options) in ExpressionResultExtensions.cs:line 151
   at JsonPathToModel.JsonPathModelNavigator.SetValue(Object model, String path, Object val) in JsonPathModelNavigator.cs:line 96
```

### Code Location
- **File**: `JsonPathToModel\Extensions\ExpressionResultExtensions.cs`
- **Line**: 151
- **Method**: `SetValue(ExpressionResult result, Object target, Object val, NavigatorConfigOptions options)`

## Impact Assessment

### Severity: **High**
- Collection element setting is a fundamental feature for JSON path manipulation
- Affects both optimized (IL emission) and non-optimized (reflection) code paths
- No current workaround exists for direct collection element modification

### Affected Scenarios
1. ❌ **Array element setting**: `$.ArrayProperty[index]`
2. ❌ **List element setting**: `$.ListProperty[index]` 
3. ❌ **Dictionary value setting**: `$.DictProperty["key"]`
4. ❌ **All collection types**: Arrays, Lists, Dictionaries, IList, ICollection
5. ❌ **Both optimization modes**: IL emission and reflection fallback

### Working Scenarios
1. ✅ **Property on collection element**: `$.ArrayProperty[index].SubProperty`
2. ✅ **Nested object properties**: `$.Users[0].Profile.Name`
3. ✅ **Regular property setting**: `$.SimpleProperty`

## Technical Analysis

### Current Implementation Gaps
1. **ExpressionEngine Limitation**: Both `GetStraightEmitterGet` and `GetStraightEmitterSetDetails` methods have collection bailout conditions:
   ```csharp
   if (tokens.Any(t => t.CollectionDetails != null)) {
       return null; // Falls back to reflection
   }
   ```

2. **Reflection Fallback Issue**: Even the reflection-based approach in `ExpressionResultExtensions.SetValue()` doesn't handle collection element setting correctly.

3. **Missing Collection Setter Logic**: No implementation exists for:
   - Array element assignment (`array[index] = value`)
   - List element assignment (`list[index] = value`) 
   - Dictionary value assignment (`dict["key"] = value`)

## Proposed Solution

### Phase 1: Reflection-Based Collection Setters
Extend `ExpressionResultExtensions.SetValue()` to handle collection element setting:

```csharp
// Handle collection element setting
if (token.CollectionDetails != null) {
    if (currentObject is Array array && token.CollectionDetails.Index.HasValue) {
        array.SetValue(val, token.CollectionDetails.Index.Value);
        return;
    }
    if (currentObject is IList list && token.CollectionDetails.Index.HasValue) {
        list[token.CollectionDetails.Index.Value] = val;
        return;
    }
    if (currentObject is IDictionary dict && !string.IsNullOrEmpty(token.CollectionDetails.Literal)) {
        dict[token.CollectionDetails.Literal] = val;
        return;
    }
}
```

### Phase 2: IL Emission Optimization
Extend `ExpressionEngine.GetStraightEmitterSetDetails()` to emit IL for collection setting:

```csharp
// Example IL emission for array[index] = value
result.LoadArgument(0);           // Load target array
result.LoadConstant(index);       // Load index
result.LoadArgument(1);           // Load value
result.StoreElement(elementType); // array[index] = value
```

## Test Coverage Requirements

### Integration Tests Needed
- [x] **Created**: `CollectionSetterIntegrationTests.cs` (13 test cases)
- [x] **Created**: `CollectionSetterOptimizationVerificationTests.cs` (5 test cases)

### Test Results Summary
- **Total Tests**: 18 collection setter tests
- **Currently Passing**: 1 test (property setting on collection element)
- **Currently Failing**: 17 tests (direct collection element setting)

## Related Issues
- **Issue #12**: Sigil Collections Support (parent feature)
- **Getter Support**: ✅ Collection element getting is already implemented and working
- **Performance**: Collection setters should achieve similar 3-4x performance improvement as getters

## Priority and Next Steps

### High Priority (Immediate)
1. Fix reflection-based collection element setting in `ExpressionResultExtensions.SetValue()`
2. Add proper error handling for out-of-bounds access based on `FailOnCollectionKeyNotFound`
3. Validate all existing tests still pass after collection setter implementation

### Medium Priority (Next Sprint)
1. Implement IL emission optimization for collection setters in `ExpressionEngine`
2. Add comprehensive performance benchmarks comparing optimized vs non-optimized setters
3. Update documentation with collection setter examples

### Code Review Checklist
- [ ] Reflection-based collection setters implemented
- [ ] IL emission collection setters implemented  
- [ ] Error handling respects `FailOnCollectionKeyNotFound` configuration
- [ ] Out-of-bounds handling matches getter behavior
- [ ] All integration tests pass
- [ ] Performance benchmarks show expected improvements
- [ ] Backwards compatibility maintained

## Testing Commands
```bash
# Run collection setter tests
dotnet test --filter "CollectionSetterIntegrationTests"
dotnet test --filter "CollectionSetterOptimizationVerificationTests"

# Run full test suite
dotnet test tests/JsonPathToModel.Tests/JsonPathToModel.Tests.csproj
```
