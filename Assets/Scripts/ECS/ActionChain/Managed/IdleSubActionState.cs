using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;

public class IdleSubActionState : ISubActionState
{
    private const float IdleTime = 2f;

    public IdleSubActionState()
    {
    }

    public void Refresh(SystemBase system)
    {
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer, ref Random random)
    {
        if (timer.IsTimeout(IdleTime))
        {
            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }
}

