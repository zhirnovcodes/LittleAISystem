using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;

public class TestIdle : ISubActionState
{

    public TestIdle()
    {
    }

    public void Refresh(SystemBase system)
    {
    }

    public void Enable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to enable for idle
    }

    public void Disable(Entity entity, Entity target, EntityCommandBuffer buffer)
    {
        // Nothing to disable for idle
    }

    public SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer)
    {
        if (timer.TimeElapsed >= 2.0f)
        {
            return SubActionResult.Success();
        }

        return SubActionResult.Running();
    }
}

