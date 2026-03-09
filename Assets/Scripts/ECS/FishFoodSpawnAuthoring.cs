using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class FishFoodSpawnAuthoring : MonoBehaviour
{
    public GameObject Prefab;

    [Header("Spawn Interval (Min, Max)")]
    public float2 SpawnInterval = new float2(5f, 10f);
    public uint Seed = 1345;

    class Baker : Baker<FishFoodSpawnAuthoring>
    {
        public override void Bake(FishFoodSpawnAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);

            var prefab = GetEntity(authoring.Prefab, TransformUsageFlags.None);

            AddComponent(entity, new FishFoodSpawnComponent
            {
                Prefab = prefab,
                SpawnInterval = authoring.SpawnInterval,
                Position = authoring.transform.position,
                Scale = authoring.transform.localScale,
                Random = new Unity.Mathematics.Random(authoring.Seed)
            });
        }
    }
}
