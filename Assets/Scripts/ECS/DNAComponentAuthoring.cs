using Unity.Entities;
using UnityEngine;

public class DNAComponentAuthoring : MonoBehaviour
{
    public GameObject DNAChild;

    class Baker : Baker<DNAComponentAuthoring>
    {
        public override void Bake(DNAComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new DNAComponent
            {
                DNA = GetEntity(authoring.DNAChild, TransformUsageFlags.Dynamic)
            });
        }
    }
}

