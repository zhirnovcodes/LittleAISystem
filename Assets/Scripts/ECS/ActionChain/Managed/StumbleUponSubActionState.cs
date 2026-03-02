using Unity.Entities;
using Unity.Transforms;

public class StumbleUponSubActionState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<AnimalStatsComponent> AnimalStatsLookup;
    private ComponentLookup<FemaleGenetaliaComponent> FemaleGenetaliaLookup;
    private ComponentLookup<MaleGenetaliaComponent> MaleGenetaliaLookup;
    private ComponentLookup<DNAComponent> DNALookup;
    private ComponentLookup<TalkingDataComponent> TalkingDataLookup;

    private const float Delta = 0.1f;

    public StumbleUponSubActionState(
        ComponentLookup<LocalTransform> transformLookup,
        ComponentLookup<AnimalStatsComponent> animalStatsLookup,
        ComponentLookup<FemaleGenetaliaComponent> femaleGenetaliaLookup,
        ComponentLookup<MaleGenetaliaComponent> maleGenetaliaLookup,
        ComponentLookup<DNAComponent> dnaLookup,
        ComponentLookup<TalkingDataComponent> talkingDataLookup)
    {
        TransformLookup = transformLookup;
        AnimalStatsLookup = animalStatsLookup;
        FemaleGenetaliaLookup = femaleGenetaliaLookup;
        MaleGenetaliaLookup = maleGenetaliaLookup;
        DNALookup = dnaLookup;
        TalkingDataLookup = talkingDataLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        AnimalStatsLookup.Update(system);
        FemaleGenetaliaLookup.Update(system);
        MaleGenetaliaLookup.Update(system);
        DNALookup.Update(system);
        TalkingDataLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to enable
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

        // Get DNA entity first
        if (!DNALookup.HasComponent(entity))
        {
            return SubActionResult.Fail(7);
        }

        var dnaEntity = DNALookup[entity].DNA;

        // Get talking data from DNA entity
        if (!TalkingDataLookup.HasComponent(dnaEntity))
        {
            return SubActionResult.Fail(7);
        }

        var talkingData = TalkingDataLookup[dnaEntity];
        float failTime = talkingData.StumbleFailTime;
        float maxDistance = talkingData.MaxDistance;

        // if time elapsed > FailTime, fail state, error code = 2
        if (timer.IsTimeout(failTime))
        {
            return SubActionResult.Fail(2);
        }

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // Check if target is reached
        if (entityTransform.IsTargetReached(targetTransform, maxDistance) == false)
        {
            return SubActionResult.Running();
        }

        // Check if looking towards target
        if (entityTransform.IsLookingTowards(targetTransform, Delta) == false)
        {
            return SubActionResult.Running();
        }

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

