using Unity.Entities;
using UnityEngine;

public class MaleGenetaliaComponentAuthoring : MonoBehaviour
{
    class Baker : Baker<MaleGenetaliaComponentAuthoring>
    {
        public override void Bake(MaleGenetaliaComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new MaleGenetaliaComponent
            {
            });
        }
    }
}

