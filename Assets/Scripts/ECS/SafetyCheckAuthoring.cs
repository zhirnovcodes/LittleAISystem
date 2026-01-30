using System;
using Unity.Entities;
using UnityEngine;

public class SafetyCheckAuthoring : MonoBehaviour
{
    [SerializeField] private SafetyCheckEntry[] SafetyChecks = Array.Empty<SafetyCheckEntry>();

    [Serializable]
    public struct SafetyCheckEntry
    {
        public ConditionFlags ActorConditions;
        public float SafetyRecession;
    }

    class Baker : Baker<SafetyCheckAuthoring>
    {
        public override void Bake(SafetyCheckAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            var buffer = AddBuffer<SafetyCheckItem>(entity);

            foreach (var check in authoring.SafetyChecks)
            {
                buffer.Add(new SafetyCheckItem
                {
                    ActorConditions = check.ActorConditions,
                    SafetyRecession = check.SafetyRecession
                });
            }
        }
    }
}

