using LittleAI.Enums;
using Unity.Entities;

public interface ISubActionState
{
    void Refresh(SystemBase system);
    void Enable(Entity entity, Entity target, EntityCommandBuffer buffer);
    void Disable(Entity entity, Entity target, EntityCommandBuffer buffer);
    SubActionResult Update(Entity entity, Entity target, EntityCommandBuffer buffer, in SubActionTimeComponent timer);
}
