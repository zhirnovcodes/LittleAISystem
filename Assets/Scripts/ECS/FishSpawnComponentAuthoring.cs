using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class FishSpawnComponentAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject Prefab;
    [SerializeField] private int Count = 10;
    [Header("Vision Range (Min, Max)")]
    [SerializeField] private float2 VisionRange = new float2(5f, 15f);
    [Header("Vision Interval (Min, Max)")]
    [SerializeField] private float2 VisionInterval = new float2(0.3f, 1.0f);
    [SerializeField] private uint RandomSeed = 1234;

    class Baker : Baker<FishSpawnComponentAuthoring>
    {
        public override void Bake(FishSpawnComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new FishSpawnComponent
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                Count = authoring.Count,
                VisionRange = authoring.VisionRange,
                VisionInterval = authoring.VisionInterval,
                RandomSeed = authoring.RandomSeed
            });
        }
    }
}

