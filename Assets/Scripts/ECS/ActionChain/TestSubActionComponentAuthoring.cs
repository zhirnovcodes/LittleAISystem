using Unity.Entities;
using UnityEngine;

public class TestSubActionComponentAuthoring : MonoBehaviour
{
    [SerializeField] private GameObject Target;

    class Baker : Baker<TestSubActionComponentAuthoring>
    {
        public override void Bake(TestSubActionComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            
            var targetEntity = Entity.Null;
            if (authoring.Target != null)
            {
                targetEntity = GetEntity(authoring.Target, TransformUsageFlags.Dynamic);
            }
            
            AddComponent(entity, new TestSubActionComponent
            {
                CurrentSubActionIndex = -1,
                Target = targetEntity
            });
        }
    }
}

