using Unity.Entities;
using Unity.Transforms;

public class CommunicateSubActionState : ISubActionState
{
    private ComponentLookup<LocalTransform> TransformLookup;
    private ComponentLookup<AnimalStatsComponent> AnimalStatsLookup;
    private ComponentLookup<StatsIncreaseComponent> StatsIncreaseLookup;
    private ComponentLookup<FemaleGenetaliaComponent> FemaleGenetaliaLookup;
    private ComponentLookup<MaleGenetaliaComponent> MaleGenetaliaLookup;

    private const float MaxDistance = 0.3f;

    public CommunicateSubActionState(
        ComponentLookup<LocalTransform> transformLookup,
        ComponentLookup<AnimalStatsComponent> animalStatsLookup,
        ComponentLookup<FemaleGenetaliaComponent> femaleGenetaliaLookup,
        ComponentLookup<MaleGenetaliaComponent> maleGenetaliaLookup,
        ComponentLookup<StatsIncreaseComponent> statsIncreaseLookup)
    {
        TransformLookup = transformLookup;
        AnimalStatsLookup = animalStatsLookup;
        StatsIncreaseLookup = statsIncreaseLookup;
        FemaleGenetaliaLookup = femaleGenetaliaLookup;
        MaleGenetaliaLookup = maleGenetaliaLookup;
    }

    public void Refresh(SystemBase system)
    {
        TransformLookup.Update(system);
        AnimalStatsLookup.Update(system);
        StatsIncreaseLookup.Update(system);
        FemaleGenetaliaLookup.Update(system);
        MaleGenetaliaLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Set genitalia IsActive to true
        if (FemaleGenetaliaLookup.HasComponent(entity))
        {
            var femaleGenitalia = FemaleGenetaliaLookup[entity];
            femaleGenitalia.IsActive = true;
            buffer.SetComponent(entity, femaleGenitalia);
        }

        if (MaleGenetaliaLookup.HasComponent(entity))
        {
            var maleGenitalia = MaleGenetaliaLookup[entity];
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

        var entityTransform = TransformLookup[entity];
        var targetTransform = TransformLookup[target];

        // Check if target is reached
        if (entityTransform.IsTargetReached(targetTransform, MaxDistance) == false)
        {
            return SubActionResult.Fail(2);
        }

        // if entity does not have StatsIncreaseComponent - return fail with code 3
        if (!StatsIncreaseLookup.HasComponent(entity))
        {
            return SubActionResult.Fail(3);
        }

        // Add stat Social with increase speed from component * delta time
        var statsIncrease = StatsIncreaseLookup[entity];
        var socialGain = statsIncrease.AnimalStats.Social * timer.DeltaTime;

        var statsChange = new AnimalStatsBuilder().WithSocial(socialGain).Build();

        buffer.AppendToBuffer(entity, new StatsChangeItem
        {
            StatsChange = statsChange
        });

        // Check if entity has stats component
        if (!AnimalStatsLookup.HasComponent(entity))
        {
            return SubActionResult.Running();
        }

        var animalStats = AnimalStatsLookup[entity];

        // If have male genitalia and Social >= 100
        if (MaleGenetaliaLookup.HasComponent(entity))
        {
            if (animalStats.Stats.Social >= 100f)
            {
                AddDNAToTarget(entity, target, buffer);
                return SubActionResult.Success();
            }
        }

        // If female genitalia and Social >= 100
        if (FemaleGenetaliaLookup.HasComponent(entity))
        {
            if (animalStats.Stats.Social >= 100f)
            {
                return SubActionResult.Success();
            }
        }

        return SubActionResult.Running();
    }

    private void AddDNAToTarget(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // TODO: Implement DNA addition logic
    }
}

