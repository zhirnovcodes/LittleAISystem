using Unity.Entities;
using UnityEngine;

public class TestAnimalSpawnAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public int AnimalsCount = 10;
    public ConditionFlags Flags;

    public class TestAnimalSpawnBaker : Baker<TestAnimalSpawnAuthoring>
    {
        public override void Bake(TestAnimalSpawnAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new TestAnimalSpawnComponent
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                AnimalsCount = authoring.AnimalsCount,
                Flags = authoring.Flags
            });
        }
    }
}

