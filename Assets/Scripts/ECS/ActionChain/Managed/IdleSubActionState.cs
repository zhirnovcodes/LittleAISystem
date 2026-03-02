using LittleAI.Enums;
using Unity.Entities;

public class IdleSubActionState : ISubActionState
{
    private ComponentLookup<DNAComponent> DNALookup;
    private ComponentLookup<MovingDataComponent> MovingDataLookup;

    private const float DefaultIdleTime = 2f;

    public IdleSubActionState(ComponentLookup<DNAComponent> dnaLookup, ComponentLookup<MovingDataComponent> movingDataLookup)
    {
        DNALookup = dnaLookup;
        MovingDataLookup = movingDataLookup;
    }

    public void Refresh(SystemBase system)
    {
        DNALookup.Update(system);
        MovingDataLookup.Update(system);
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer)
    {
        // Get moving data from DNA entity, use default if not found
        float idleTime = DefaultIdleTime;
        
        if (DNALookup.HasComponent(entity))
        {
            var dnaEntity = DNALookup[entity].DNA;
            if (MovingDataLookup.HasComponent(dnaEntity))
            {
                var movingData = MovingDataLookup[dnaEntity];
                idleTime = movingData.IdleTime;
            }
        }

        if (timer.IsTimeout(idleTime))
        {
            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }
}

