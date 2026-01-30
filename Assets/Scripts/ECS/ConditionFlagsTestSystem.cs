using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct ConditionFlagsTestSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<RunConditionTestComponent>();
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        Debug.Log("=== Starting ConditionFlags Tests ===");

        TestCase1_NoConditions();
        TestCase2_SingleCondition();
        TestCase3_MultipleConditions();
        TestCase4_ConditionNotMet();
        TestCase5_PartialConditionMatch();
        TestCase6_AllConditionsMatch();

        Debug.Log("=== All ConditionFlags Tests Complete ===");
    }

    private void TestCase1_NoConditions()
    {
        Debug.Log("\n--- Test Case 1: No expected conditions (should always pass) ---");
        
        var actorConditions = ConditionFlags.IsAnimal | ConditionFlags.IsHerbivore;
        var expectedConditions = ConditionFlags.None;
        
        bool result = actorConditions.IsConditionMet(expectedConditions);
        
        AssertTrue(result, "TestCase1");
        Debug.Log($"Test Case 1: Expected true, Got {result} - {(result ? "PASSED" : "FAILED")}");
    }

    private void TestCase2_SingleCondition()
    {
        Debug.Log("\n--- Test Case 2: Single condition check (should pass) ---");
        
        var actorConditions = ConditionFlags.IsAnimal | ConditionFlags.IsHerbivore;
        var expectedConditions = ConditionFlags.IsAnimal;
        
        bool result = actorConditions.IsConditionMet(expectedConditions);
        
        AssertTrue(result, "TestCase2");
        Debug.Log($"Test Case 2: Expected true, Got {result} - {(result ? "PASSED" : "FAILED")}");
    }

    private void TestCase3_MultipleConditions()
    {
        Debug.Log("\n--- Test Case 3: Multiple conditions (all present, should pass) ---");
        
        var actorConditions = ConditionFlags.IsAnimal | ConditionFlags.IsHerbivore | ConditionFlags.IsPlant;
        var expectedConditions = ConditionFlags.IsAnimal | ConditionFlags.IsHerbivore;
        
        bool result = actorConditions.IsConditionMet(expectedConditions);
        
        AssertTrue(result, "TestCase3");
        Debug.Log($"Test Case 3: Expected true, Got {result} - {(result ? "PASSED" : "FAILED")}");
    }

    private void TestCase4_ConditionNotMet()
    {
        Debug.Log("\n--- Test Case 4: Condition not met (should fail) ---");
        
        var actorConditions = ConditionFlags.IsAnimal | ConditionFlags.IsHerbivore;
        var expectedConditions = ConditionFlags.IsPredator;
        
        bool result = actorConditions.IsConditionMet(expectedConditions);
        
        AssertFalse(result, "TestCase4");
        Debug.Log($"Test Case 4: Expected false, Got {result} - {(!result ? "PASSED" : "FAILED")}");
    }

    private void TestCase5_PartialConditionMatch()
    {
        Debug.Log("\n--- Test Case 5: Partial condition match (should fail) ---");
        
        var actorConditions = ConditionFlags.IsAnimal | ConditionFlags.IsHerbivore;
        var expectedConditions = ConditionFlags.IsAnimal | ConditionFlags.IsPredator;
        
        bool result = actorConditions.IsConditionMet(expectedConditions);
        
        AssertFalse(result, "TestCase5");
        Debug.Log($"Test Case 5: Expected false, Got {result} - {(!result ? "PASSED" : "FAILED")}");
    }

    private void TestCase6_AllConditionsMatch()
    {
        Debug.Log("\n--- Test Case 6: All conditions match exactly (should pass) ---");
        
        var actorConditions = ConditionFlags.IsAnimal | ConditionFlags.IsHerbivore | ConditionFlags.IsPlant | ConditionFlags.IsPredator;
        var expectedConditions = ConditionFlags.IsAnimal | ConditionFlags.IsHerbivore | ConditionFlags.IsPlant | ConditionFlags.IsPredator;
        
        bool result = actorConditions.IsConditionMet(expectedConditions);
        
        AssertTrue(result, "TestCase6");
        Debug.Log($"Test Case 6: Expected true, Got {result} - {(result ? "PASSED" : "FAILED")}");
    }

    private void AssertTrue(bool value, string testName)
    {
        if (!value)
        {
            Debug.LogError($"FAILED: {testName} - Expected: true, Got: {value}");
        }
        else
        {
            Debug.Log($"PASSED: {testName}");
        }
    }

    private void AssertFalse(bool value, string testName)
    {
        if (value)
        {
            Debug.LogError($"FAILED: {testName} - Expected: false, Got: {value}");
        }
        else
        {
            Debug.Log($"PASSED: {testName}");
        }
    }
}

