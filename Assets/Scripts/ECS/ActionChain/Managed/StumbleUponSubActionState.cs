using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class StumbleUponSubActionState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<AnimalStatsComponent> AnimalStatsLookup;
    private ComponentLookup<GenetaliaComponent> GenetaliaLookup;

    private const float FailTime = 5f;
    private const float MaxDistance = 0.3f;
    private const float Delta = 1f;

    public StumbleUponSubActionState(
        ComponentLookup<LocalTransform> transformLookup,
        ComponentLookup<AnimalStatsComponent> animalStatsLookup,
        ComponentLookup<GenetaliaComponent> genetaliaLookup)
    {
        TransformLookup = transformLookup;
        AnimalStatsLookup = animalStatsLookup;
        GenetaliaLookup = genetaliaLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        AnimalStatsLookup.Update(system);
        GenetaliaLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        if (GenetaliaLookup.HasComponent(entity))
        {
            var genitalia = GenetaliaLookup[entity];
            genitalia.IsEnabled = true;
            buffer.SetComponent(entity, genitalia);
        }
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Disable genitalia component
        if (GenetaliaLookup.HasComponent(entity))
        {
            var genitalia = GenetaliaLookup[entity];
            genitalia.IsEnabled = false;
            buffer.SetComponent(entity, genitalia);
        }
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
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

        // if time elapsed > FailTime, fail state, error code = 2
        if (timer.IsTimeout(FailTime))
        {
            return SubActionResult.Fail(2);
        }
        /*

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];
        // Check if target is not reached
        if (entityTransform.IsTargetReached(targetTransform, MaxDistance) == false)
        {
            return SubActionResult.Running();
        }

        // Check if looking towards target
        if (entityTransform.IsLookingTowards(targetTransform, Delta) == false)
        {
            return SubActionResult.Running();
        }*/

        // Check if entity has genitalia and enable it
        if (!GenetaliaLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        var genitalia = GenetaliaLookup[entity];

        // If Social >= 100, fail (already satisfied)
        if (AnimalStatsLookup.HasComponent(entity))
        {
            var animalStats = AnimalStatsLookup[entity];
            if (animalStats.Stats.Social >= 100f)
            {
                return SubActionResult.Fail(4);
            }
        }

        // Check if target has genitalia and is opposite sex
        if (!GenetaliaLookup.HasComponent(target))
        {
            return SubActionResult.Fail(5);
        }

        var targetGenitalia = GenetaliaLookup[target];
        
        // Check if opposite sex (male with female or female with male)
        if (genitalia.IsMale != targetGenitalia.IsMale)
        {
            // Check if target's genitalia is enabled
            if (targetGenitalia.IsEnabled)
            {
                return SubActionResult.Success();
            }
            
            return SubActionResult.Running();
        }

        return SubActionResult.Fail(6);
    }
}

