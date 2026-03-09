using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class FishSpawnAuthoring : MonoBehaviour
{
    [Header("Spawn Interval (Min, Max)")]
    [SerializeField] private float2 SpawnInterval = new float2(5f, 10f);
    [SerializeField] private uint RandomSeed = 1234;
    [SerializeField] private uint MaxCount = 5000;
    public List<ParentDNAAuthoring> Parents = new List<ParentDNAAuthoring>();

    class Baker : Baker<FishSpawnAuthoring>
    {
        public override void Bake(FishSpawnAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var transform = GetComponent<Transform>();

            var buffer = AddBuffer<WorldOriginItem>(entity);

            foreach (var parent in authoring.Parents)
            {
                if (parent != null)
                {
                    var parentEntity = GetEntity(parent, TransformUsageFlags.Dynamic);
                    buffer.Add(new WorldOriginItem
                    {
                        Parent = parentEntity
                    });
                }
            }

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
