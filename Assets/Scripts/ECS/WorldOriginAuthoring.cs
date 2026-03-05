using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class WorldOriginAuthoring : MonoBehaviour
{
    public List<ParentDNAAuthoring> Parents = new List<ParentDNAAuthoring>();
    
    class Baker : Baker<WorldOriginAuthoring>
    {
        public override void Bake(WorldOriginAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            
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
        }
    }
}
