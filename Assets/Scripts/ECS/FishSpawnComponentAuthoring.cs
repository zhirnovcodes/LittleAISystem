using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class FishSpawnComponentAuthoring : MonoBehaviour
{
    [Header("Spawn Interval (Min, Max)")]
    [SerializeField] private float2 SpawnInterval = new float2(5f, 10f);
    [SerializeField] private uint RandomSeed = 1234;
    [SerializeField] private uint MaxCount = 5000;

    class Baker : Baker<FishSpawnComponentAuthoring>
    {
        public override void Bake(FishSpawnComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var transform = GetComponent<Transform>();

            AddComponent(entity, new FishSpawnComponent
            {
                SpawnInterval = authoring.SpawnInterval,
                TimeElapsed = 0f,
                SpawnPosition = transform.position,
                MaxCount = authoring.MaxCount,
                Random = Unity.Mathematics.Random.CreateFromIndex(authoring.RandomSeed)
            });
        }
    }
}
