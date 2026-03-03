using LittleAI.Enums;
using Unity.Entities;
using Unity.Mathematics;

public class ActionChainAuthoring : UnityEngine.MonoBehaviour
{
    public uint Seed = 1;
    
    class Baker : Baker<ActionChainAuthoring>
    {
        public override void Bake(ActionChainAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // Add ActionRunnerComponent with idle state
            AddComponent(entity, new ActionRunnerComponent
            {
                Target = entity,
                Action = ActionTypes.Idle,
                CurrentSubActionIndex = 0,
                IsCancellationRequested = false
            });

            // Add ActionChainItem buffer
            AddBuffer<ActionChainItem>(entity);

            // Add SubActionTimeComponent
            AddComponent(entity, new SubActionTimeComponent());
            
            // Add ActionRandomComponent
            AddComponent(entity, new ActionRandomComponent
            {
                Random = Random.CreateFromIndex(authoring.Seed)
            });
        }
    }
}

