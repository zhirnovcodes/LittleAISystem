# Genome Builder Tests

This document describes the comprehensive test suite for the `AnimalGenomeBuilder` and all `GenomeType` implementations.

## Overview

The test suite verifies that:
1. Each `IGenomeDataConvertible` implementation correctly creates `GenomeData`
2. The `AnimalGenomeBuilder.WithGenome()` method correctly processes each `GenomeType`
3. All necessary components and buffers are created with the correct data

## Test Files

- **GenomeBuilderTestSystem.cs** - The main test system that runs all tests
- **RunGenomeBuilderTestComponent.cs** - Component to trigger the tests
- **RunGenomeBuilderTestAuthoring.cs** - Authoring component for easy testing in Unity

## How to Run the Tests

1. Add the `RunGenomeBuilderTestAuthoring` component to any GameObject in your scene
2. Enter Play mode in Unity
3. The tests will run automatically in the `InitializationSystemGroup`
4. Check the Unity Console for test results

## Test Coverage

### 1. StatsIncrease GenomeType
- **Convertible**: `StatsIncreaseGenomeData`
- **Component Created**: `StatsIncreaseComponent`
- **Tests**:
  - ✓ GenomeData conversion preserves AnimalStats data
  - ✓ Component is added to entity
  - ✓ Component contains correct stats values

### 2. Speed GenomeType
- **Convertible**: `SpeedGenomeData`
- **Component Created**: `MovingSpeedComponent`
- **Tests**:
  - ✓ GenomeData conversion preserves MaxSpeed and MaxRotationSpeed
  - ✓ Component is added to entity
  - ✓ Component contains correct speed values

### 3. Aging GenomeType
- **Convertible**: `AgingGenomeData`
- **Component Created**: `AgingComponent`
- **Tests**:
  - ✓ GenomeData conversion preserves MinSize and MaxSize
  - ✓ Component is added to entity
  - ✓ Component contains correct size values

### 4. Vision GenomeType
- **Convertible**: `VisionGenomeData`
- **Components Created**: 
  - `VisionComponent`
  - `VisibleItem` buffer
- **Tests**:
  - ✓ GenomeData conversion preserves MaxDistance and Interval
  - ✓ VisionComponent is added to entity
  - ✓ VisibleItem buffer is created
  - ✓ Component contains correct vision values
  - ✓ TimeElapsed is initialized to 0

### 5. NeedsBased GenomeType
- **Convertible**: `NeedsBasedGenomeData`
- **Components Created**: 
  - `NeedsActionChainComponent`
  - `NeedBasedInputItem` buffer
  - `NeedBasedOutputComponent`
- **Tests**:
  - ✓ GenomeData conversion preserves CancelThreshold and AddThreshold
  - ✓ NeedsActionChainComponent is added to entity
  - ✓ NeedBasedInputItem buffer is created
  - ✓ NeedBasedOutputComponent is created
  - ✓ Component contains correct threshold values

### 6. Stats GenomeType
- **Convertible**: `StatsGenomeData`
- **Components Created**: 
  - `AnimalStatsComponent`
  - `StatsChangeItem` buffer
- **Tests**:
  - ✓ GenomeData conversion preserves all AnimalStats data
  - ✓ AnimalStatsComponent is added to entity
  - ✓ StatsChangeItem buffer is created
  - ✓ Component contains correct stats values (Energy, Fullness, Toilet, Social, Safety, Health)

### 7. ActionChain GenomeType
- **Convertible**: `ActionChainGenomeData`
- **Components Created**: 
  - `ActionRunnerComponent`
  - `ActionChainItem` buffer
  - `SubActionTimeComponent`
- **Tests**:
  - ✓ GenomeData conversion works correctly
  - ✓ ActionRunnerComponent is added to entity
  - ✓ ActionChainItem buffer is created
  - ✓ SubActionTimeComponent is created

### 8. Advertiser GenomeType
- **Convertible**: `AdvertiserGenomeData`
- **Components Created**: 
  - `StatAdvertiserItem` buffer (with items appended)
- **Tests**:
  - ✓ GenomeData conversion combines ActorConditions and ActionType into Index
  - ✓ GenomeData conversion preserves AdvertisedValue stats
  - ✓ StatAdvertiserItem buffer is created
  - ✓ Buffer item contains correct AdvertisedValue
  - ✓ Buffer item contains correct ActorConditions (extracted from Index)
  - ✓ Buffer item contains correct ActionType (extracted from Index)
  - ✓ Multiple advertisers can be added to the same entity (buffer appending works)

### 9. Reproduction GenomeType
- **Convertible**: `ReproductionGenomeData`
- **Components Created**: 
  - `GenetaliaComponent` (for interaction behavior)
  - `ReproductionComponent` (IEnableableComponent, for gestation system)
  - `DNAStorageItem` buffer (only for female entities)
- **Tests**:
  - ✓ GenomeData conversion for male (IsMale = true)
  - ✓ GenomeData conversion for female (IsMale = false)
  - ✓ GestationTime is properly stored and retrieved
  - ✓ Both GenetaliaComponent and ReproductionComponent are added to entity
  - ✓ Male entities do NOT have DNAStorageItem buffer
  - ✓ Female entities DO have DNAStorageItem buffer
  - ✓ Component IsMale flags are correct
  - ✓ Component GestationTime is correct

### 10. StatAttenuation GenomeType
- **Convertible**: `StatAttenuationGenomeData`
- **Components Created**: 
  - `AnimalStatsAttenuationComponent` (only created once, then updated)
- **Tests**:
  - ✓ GenomeData conversion preserves StatType in Index
  - ✓ GenomeData conversion preserves Needs curve (points and tangents)
  - ✓ GenomeData conversion preserves Distance curve (points and tangents)
  - ✓ AnimalStatsAttenuationComponent is created on first attenuation
  - ✓ Attenuation data is correctly set for the specified StatType
  - ✓ Multiple stat attenuations can be set on the same entity (component updating works)
  - ✓ Component contains correct attenuation data for each stat type

## Test Data Examples

### Example: Testing StatsIncrease
```csharp
var testData = new StatsIncreaseGenomeData
{
    AnimalStats = new AnimalStats
    {
        Stats = new float4x2(
            new float4(10f, 20f, 30f, 40f), // Energy, Fullness, Toilet, Social
            new float4(50f, 60f, 0f, 0f)    // Safety, Health
        )
    }
};

// Convert to GenomeData
GenomeData genomeData = testData.GetGenomeData();

// Build with AnimalGenomeBuilder
var builder = new AnimalGenomeBuilder(commandBuffer, entity);
builder.WithGenome(GenomeType.StatsIncrease, testData);
builder.Build();
```

### Example: Testing Advertiser with Bit Packing
```csharp
var testData = new AdvertiserGenomeData
{
    AdvertisedValue = new AnimalStats { /* ... */ },
    ActorConditions = (ConditionFlags)0x1234,
    ActionType = ActionTypes.Eat
};

// GenomeData.Index will be: (0x1234 << 8) | (int)ActionTypes.Eat
// This allows both values to be stored in a single integer
```

### Example: Testing StatAttenuation
```csharp
var testData = new StatAttenuationGenomeData
{
    StatType = StatType.Energy,
    Attenuation = new AnimalStatsAttenuation
    {
        Needs = new HermiteCurve
        {
            points = new float4(0f, 1f, 100f, 0f),
            tangents = new float2(-0.01f, -0.01f)
        },
        Distance = new HermiteCurve
        {
            points = new float4(0f, 1f, 50f, 0.5f),
            tangents = new float2(-0.02f, -0.02f)
        }
    }
};
```

## Test Architecture

The tests follow this pattern for each GenomeType:

1. **Create Test Data**: Instantiate the appropriate `IGenomeDataConvertible` implementation with test values
2. **Test Conversion**: Call `GetGenomeData()` and verify the resulting `GenomeData` structure
3. **Test Builder**: Use `AnimalGenomeBuilder.WithGenome()` to add the genome to an entity
4. **Verify Components**: Check that all expected components and buffers were created
5. **Verify Data**: Read back component data and verify it matches the original test values
6. **Cleanup**: Destroy test entities to avoid pollution

## Assertion Methods

The test system provides several assertion helpers:

- `Assert(bool condition, string message)` - Basic boolean assertion
- `AssertEqual(int actual, int expected, string testName)` - Integer equality
- `AssertApprox(float actual, float expected, string testName, float tolerance = 0.001f)` - Float approximation
- `AssertApprox(float4 actual, float4 expected, string testName, float tolerance = 0.001f)` - Vector approximation

## Special Test Cases

### Buffer Creation vs Appending
- **Vision, NeedsBased, Stats, ActionChain**: Create new buffers
- **Advertiser**: Creates buffer on first call, then appends items on subsequent calls
- **StatAttenuation**: Creates component on first call, then updates it on subsequent calls

### Conditional Component Creation
- **Reproduction**: `DNAStorageItem` buffer is only created for female entities (IsMale = false)

### Bit Packing
- **Advertiser**: Uses bit packing to store both `ActorConditions` and `ActionType` in a single Index field
  - Lower byte (bits 0-7): ActionType
  - Upper bits (bits 8+): ActorConditions

### Data Transformations
- **StatAttenuation**: Converts `HermiteCurve` structures into a `float4x4` matrix for storage
- **All Stats-based**: Convert `float4x2` AnimalStats to `float4x4` for GenomeData storage

## Console Output

When tests run, you'll see output like:
```
=== Starting GenomeBuilder Tests ===

--- Testing StatsIncrease GenomeType ---
PASSED: StatsIncrease Index
PASSED: StatsIncrease Data c0
PASSED: StatsIncrease Data c1
PASSED: StatsIncrease: Should have StatsIncreaseComponent
PASSED: StatsIncrease Component c0
PASSED: StatsIncrease Component c1
✓ StatsIncrease GenomeType test passed

...

=== All GenomeBuilder Tests Complete ===
```

## Extending the Tests

To add a new GenomeType test:

1. Add a new test method following the naming pattern: `TestXxxGenome(ref SystemState state)`
2. Create test data for your new `IGenomeDataConvertible` implementation
3. Verify GenomeData conversion
4. Test component creation using `AnimalGenomeBuilder`
5. Verify all components and buffers
6. Verify data correctness
7. Clean up test entities
8. Call your new test method from `OnUpdate()`

## Notes

- Tests run once per session when entering Play mode
- The system disables itself after running to prevent repeated execution
- All test entities are cleaned up automatically
- Tests use temporary `EntityCommandBuffer` instances that are properly disposed
- Float comparisons use a tolerance of 0.001f by default to handle floating-point precision

