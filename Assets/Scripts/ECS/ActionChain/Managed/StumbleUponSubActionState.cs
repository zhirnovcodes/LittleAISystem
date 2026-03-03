using Unity.Entities;
using Unity.Transforms;

public class StumbleUponSubActionState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<AnimalStatsComponent> AnimalStatsLookup;
    private ComponentLookup<FemaleGenetaliaComponent> FemaleGenetaliaLookup;
    private ComponentLookup<MaleGenetaliaComponent> MaleGenetaliaLookup;

    private const float FailTime = 5f;
    private const float MaxDistance = 0.3f;
    private const float Delta = 1f;

    public StumbleUponSubActionState(
        ComponentLookup<LocalTransform> transformLookup,
        ComponentLookup<AnimalStatsComponent> animalStatsLookup,
        ComponentLookup<FemaleGenetaliaComponent> femaleGenetaliaLookup,
        ComponentLookup<MaleGenetaliaComponent> maleGenetaliaLookup)
    {
        TransformLookup = transformLookup;
        AnimalStatsLookup = animalStatsLookup;
        FemaleGenetaliaLookup = femaleGenetaliaLookup;
        MaleGenetaliaLookup = maleGenetaliaLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        AnimalStatsLookup.Update(system);
        FemaleGenetaliaLookup.Update(system);
        MaleGenetaliaLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        if (FemaleGenetaliaLookup.TryGetComponent(entity, out var femaleGenitalia))
        {
            femaleGenitalia.IsActive = true;
            buffer.SetComponent(entity, femaleGenitalia);
        }

        if (MaleGenetaliaLookup.TryGetComponent(entity, out var maleGenitalia))
        {
            maleGenitalia.IsActive = true;
            buffer.SetComponent(entity, maleGenitalia);
        }
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Set genitalia IsActive to false
        if (FemaleGenetaliaLookup.HasComponent(entity))
        {
            var femaleGenitalia = FemaleGenetaliaLookup[entity];
            femaleGenitalia.IsActive = false;
            buffer.SetComponent(entity, femaleGenitalia);
        }

        if (MaleGenetaliaLookup.HasComponent(entity))
        {
            var maleGenitalia = MaleGenetaliaLookup[entity];
            maleGenitalia.IsActive = false;
            buffer.SetComponent(entity, maleGenitalia);
        }
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

        // Check if entity has male or female genitalia and set IsActive to true
        bool hasMaleGenitalia = false;
        bool hasFemaleGenitalia = false;

        if (FemaleGenetaliaLookup.HasComponent(entity))
        {
            var femaleGenitalia = FemaleGenetaliaLookup[entity];
            femaleGenitalia.IsActive = true;
            buffer.SetComponent(entity, femaleGenitalia);
            hasFemaleGenitalia = true;
        }
        
        if (MaleGenetaliaLookup.HasComponent(entity))
        {
            var maleGenitalia = MaleGenetaliaLookup[entity];
            maleGenitalia.IsActive = true;
            buffer.SetComponent(entity, maleGenitalia);
            hasMaleGenitalia = true;
        }

        // If entity doesn't have genitalia, just return running
        if (hasFemaleGenitalia == false && hasMaleGenitalia == false)
        {
            return SubActionResult.Fail(3);
        }

        // If Social >= 100, fail (already satisfied)
        if (AnimalStatsLookup.HasComponent(entity))
        {
            var animalStats = AnimalStatsLookup[entity];
            if (animalStats.Stats.Social >= 100f)
            {
                return SubActionResult.Fail(4);
            }
        }

        if (FemaleGenetaliaLookup.HasComponent(target))
        {
            if (hasMaleGenitalia)
            {
                var targetFemaleGenitalia = FemaleGenetaliaLookup[target];
                if (targetFemaleGenitalia.IsActive)
                {
                    return SubActionResult.Success();
                }

                return SubActionResult.Running();
            }
        }
        
        if (MaleGenetaliaLookup.HasComponent(target))
        {
            if (hasFemaleGenitalia)
            {
                var targetMaleGenitalia = MaleGenetaliaLookup[target];
                if (targetMaleGenitalia.IsActive)
                {
                    return SubActionResult.Success();
                }
                
                return SubActionResult.Running();
            }
        }

        return SubActionResult.Fail(6);
    }
}

