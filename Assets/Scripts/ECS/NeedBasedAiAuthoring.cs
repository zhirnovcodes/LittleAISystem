using LittleAI.Enums;
using Unity.Entities;
using UnityEngine;

public class NeedBasedAiAuthoring : MonoBehaviour
{
    [Header("Action Chain Manipulation")]
    [SerializeField] private bool EnableActionChainManipulation = false;
    [SerializeField] private float CancelThreshold = 0.5f;
    [SerializeField] private float AddThreshold = 0.7f;

    class Baker : Baker<NeedBasedAiAuthoring>
    {
        public override void Bake(NeedBasedAiAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // Add NeedBasedInputItem buffer
            AddBuffer<NeedBasedInputItem>(entity);

            // Add NeedBasedOutputComponent
            AddComponent(entity, new NeedBasedOutputComponent
            {
                Target = Entity.Null,
                Action = ActionTypes.Idle,
                StatsWeight = 0f
            });

            // Optionally add NeedsActionChainComponent
            if (authoring.EnableActionChainManipulation)
            {
                AddComponent(entity, new NeedsActionChainComponent
                {
                    CancelThreshold = authoring.CancelThreshold,
                    AddThreshold = authoring.AddThreshold
                });

                SetComponentEnabled<NeedsActionChainComponent>(entity, true);
            }
        }
    }
}

