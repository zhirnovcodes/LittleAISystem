using LittleAI.Enums;
using Unity.Entities;

public class IdleSubActionState : ISubActionState
{
    private ComponentLookup<MovingDataComponent> MovingDataLookup;

    private const float DefaultIdleTime = 2f;

    public IdleSubActionState(ComponentLookup<MovingDataComponent> movingDataLookup)
    {
        MovingDataLookup = movingDataLookup;
    }

    public void Refresh(SystemBase system)
    {
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
        // Get moving data from entity, use default if not found
        float idleTime = DefaultIdleTime;
        
        if (MovingDataLookup.HasComponent(entity))
        {
            var movingData = MovingDataLookup[entity];
            idleTime = movingData.IdleTime;
        }

        if (timer.IsTimeout(idleTime))
        {
            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }
}

