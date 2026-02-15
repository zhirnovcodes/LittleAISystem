using LittleAI.Enums;
using Unity.Entities;
using UnityEngine;

public class ActionChainAuthoring : MonoBehaviour
{
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
        }
    }
}

