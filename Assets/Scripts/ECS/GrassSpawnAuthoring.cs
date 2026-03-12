using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public struct MapGrid
{
    public float3 Position;
    public int3 Size;       // cell count per axis
    public float3 CellSize;
 
    public float3 GetCellCenterWorld(int3 index)
    {
        return Position + new float3(
            index.x * CellSize.x + CellSize.x * 0.5f,
            index.y * CellSize.y + CellSize.y * 0.5f,
            index.z * CellSize.z + CellSize.z * 0.5f
        );
    }
 
    public float3 GetCellCenterWorldHex(int3 index)
    {
        var center = GetCellCenterWorld(index);
        var offset = new float3(0f, 0f, CellSize.z * 0.5f);
        center += index.x % 2 != 0 ? offset : float3.zero;
        return center;
    }
}

// =========================================================================
// Component
// =========================================================================

public struct GrassSpawnComponent : IComponentData, IEnableableComponent
{
    public MapGrid Grid;
    public float Chance;
    public Unity.Mathematics.Random Random;
    public Entity Prefab;
}

// =========================================================================
// Authoring
// =========================================================================

public class GrassSpawnAuthoring : MonoBehaviour
{
    public Grid Grid;
    public Vector3Int GridSize;
    [Range(0f, 1f)] public float Chance = 0.5f;
    public uint RandomSeed = 1;
    public GameObject Prefab;

    public class Baker : Baker<GrassSpawnAuthoring>
    {
        public override void Bake(GrassSpawnAuthoring authoring)
        {
            if (authoring.Grid == null || authoring.Prefab == null)
                return;

            var entity = GetEntity(TransformUsageFlags.None);
            var cellSize = authoring.Grid.cellSize;

            AddComponent(entity, new GrassSpawnComponent
            {
                Grid = new MapGrid
                {
                    Position = authoring.transform.position,
                    Size = new int3(authoring.GridSize.x, authoring.GridSize.y, authoring.GridSize.z),
                    CellSize = new float3(cellSize.x, cellSize.y, cellSize.z)
                },
                Chance = authoring.Chance,
                Random = Unity.Mathematics.Random.CreateFromIndex(authoring.RandomSeed),
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}

// =========================================================================
// System
// =========================================================================

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial class GrassSpawnSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<GrassSpawnComponent>();
    }

    protected override void OnUpdate()
    {
        var buffer = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

        Entities
            .WithAll<GrassSpawnComponent>()
            .ForEach((Entity entity, ref GrassSpawnComponent spawn) =>
            {
                SpawnGrass(ref spawn, ref buffer);

                buffer.SetComponentEnabled<GrassSpawnComponent>(entity, false);
            })
            .WithoutBurst()
            .Run();

        buffer.Playback(EntityManager);
        buffer.Dispose();
    }

    private void SpawnGrass(ref GrassSpawnComponent spawn, ref EntityCommandBuffer buffer)
    {
        var grid = spawn.Grid;

        for (int x = 0; x < grid.Size.x; x++)
        {
            for (int z = 0; z < grid.Size.z; z++)
            {
                if (spawn.Random.NextFloat() > spawn.Chance)
                    continue;

                var cellIndex = new int3(x, 0, z);
                var worldPosition = grid.GetCellCenterWorldHex(cellIndex);

                var instance = buffer.Instantiate(spawn.Prefab);
                buffer.SetComponent(instance, LocalTransform.FromPosition(worldPosition));
            }
        }
    }
}
