using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PrefabLibraryAuthoring : MonoBehaviour
{
    [System.Serializable]
    public struct PrefabEntry
    {
        public ConditionFlags Flag;
        public GameObject Prefab;
    }

    public List<PrefabEntry> PrefabEntries = new List<PrefabEntry>();

    public class Baker : Baker<PrefabLibraryAuthoring>
    {
        public override void Bake(PrefabLibraryAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            var buffer = AddBuffer<PrefabLibraryItem>(entity);

            foreach (var entry in authoring.PrefabEntries)
            {
                buffer.Add(new PrefabLibraryItem
                {
                    Flag = entry.Flag,
                    Prefab = GetEntity(entry.Prefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}

