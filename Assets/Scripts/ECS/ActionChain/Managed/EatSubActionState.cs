using Unity.Entities;
using Unity.Transforms;

public class EatSubActionState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<EdibleComponent> EdibleLookup;
    private ComponentLookup<AnimalStatsComponent> AnimalStatsLookup;
    private ComponentLookup<DNAComponent> DNALookup;
    private ComponentLookup<EatDataComponent> EatDataLookup;

    public EatSubActionState(ComponentLookup<LocalTransform> transformLookup, ComponentLookup<EdibleComponent> edibleLookup, ComponentLookup<AnimalStatsComponent> animalStatsLookup, ComponentLookup<DNAComponent> dnaLookup, ComponentLookup<EatDataComponent> eatDataLookup)
    {
        TransformLookup = transformLookup;
        EdibleLookup = edibleLookup;
        AnimalStatsLookup = animalStatsLookup;
        DNALookup = dnaLookup;
        EatDataLookup = eatDataLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        EdibleLookup.Update(system);
        AnimalStatsLookup.Update(system);
        DNALookup.Update(system);
        EatDataLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to enable for eat
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to disable for eat
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer)
    {
        // if actor entity does not exist in transform lookup, fail state. code = 0
        if (!TransformLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(0);
        }

        // if target does not exist in transform lookup, fail state. code = 1
        if (!TransformLookup.HasComponent(target))
        {
            return SubActionResult.Fail(1);
        }

        // Get DNA entity first
        if (!DNALookup.HasComponent(entity))
        {
            return SubActionResult.Fail(8);
        }

        var dnaEntity = DNALookup[entity].DNA;

        // Get eat data from DNA entity
        if (!EatDataLookup.HasComponent(dnaEntity))
        {
            return SubActionResult.Fail(8);
        }

        var eatData = EatDataLookup[dnaEntity];
        float interval = eatData.Interval;
        float failTime = eatData.FailTime;
        float maxDistance = eatData.MaxDistance;
        float biteSize = eatData.BiteSize;

        // if time elapsed > FailTime, fail state, error code = 2
        if (timer.IsTimeout(failTime))
        {
            return SubActionResult.Fail(2);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // if distance between transforms > MaxDistance - fail with error code 3
        if (!entityTransform.IsTargetReached(targetTransform, maxDistance))
        {
            return SubActionResult.Fail(3);
        }

        // if target does not exist in EdibleBody lookup, fail state. code = 4
        if (!EdibleLookup.HasComponent(target))
        {
            return SubActionResult.Fail(4);
        }

        var edibleComponent = EdibleLookup[target];

        // if target.edible component.EdibleBody does not exist in Transform lookup, fail state. code = 5
        if (!TransformLookup.HasComponent(edibleComponent.EdibleBody))
        {
            return SubActionResult.Fail(5);
        }

        var edibleBodyTransform = TransformLookup[edibleComponent.EdibleBody];

        // if target.edible scale <= 0 -> return fail with code 6
        if (edibleBodyTransform.Scale <= 0)
        {
            return SubActionResult.Fail(6);
        }

        // if animal does not have AnimalStatsComponent - return fail with code 7
        if (!AnimalStatsLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(7);
        }

        // Check if it's time for another bite using IsTimerTick
        if (timer.IsTimerTick(interval, true))
        {
            Bite(entity, target, edibleComponent, edibleBodyTransform, buffer, biteSize);
        }

        // Check if Fullness >= 100 - returns Success
        var animalStats = AnimalStatsLookup[entity];
        if (animalStats.Stats.Fullness >= 100f)
        {
            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }

    private void Bite(Entity entity, Entity target, EdibleComponent edibleComponent, LocalTransform edibleBodyTransform, EntityCommandBuffer buffer, float biteSize)
    {
        // Calculate actual bite size (might be less than biteSize if remaining is smaller)
        var actualBiteSize = biteSize;
        var newScale = edibleBodyTransform.Scale - biteSize;
        
        if (newScale < 0)
        {
            actualBiteSize = edibleBodyTransform.Scale;
            newScale = 0;
        }

        // Update the scale of EdibleBody
        edibleBodyTransform.Scale = newScale;
        buffer.SetComponent(edibleComponent.EdibleBody, edibleBodyTransform);

        // Calculate nutrition gained from this bite
        var nutritionGained = actualBiteSize * edibleComponent.Nutrition;

        // Add StatsChangeItem with Fullness
        var statsChange = new AnimalStatsBuilder().WithFullness(nutritionGained).Build();

        buffer.AppendToBuffer(entity, new StatsChangeItem
        {
            StatsChange = statsChange
        });

        // if after bite scale is <= 0 - destroys target
        if (newScale <= 0)
        {
            buffer.DestroyEntity(target);
            // Could return success here, but let's continue to check nutrition
        }
    }
}

