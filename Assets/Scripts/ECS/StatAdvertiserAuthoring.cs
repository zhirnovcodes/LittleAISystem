using LittleAI.Enums;
using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public class StatAdvertiserItemData
{
    public AnimalStats AdvertisedValue;
    public ConditionFlags ActorConditions;
    public ActionTypes ActionType;
}

public class StatAdvertiserAuthoring : MonoBehaviour
{
    [SerializeField] private StatAdvertiserItemData[] StatAdvertiserItems;

    class Baker : Baker<StatAdvertiserAuthoring>
    {
        public override void Bake(StatAdvertiserAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // Add StatAdvertiserItem buffer
            var buffer = AddBuffer<StatAdvertiserItem>(entity);

            // Add all configured items to the buffer
            if (authoring.StatAdvertiserItems != null)
            {
                foreach (var item in authoring.StatAdvertiserItems)
                {
                    buffer.Add(new StatAdvertiserItem
                    {
                        AdvertisedValue = item.AdvertisedValue,
                        ActorConditions = item.ActorConditions,
                        ActionType = item.ActionType
                    });
                }
            }
        }
    }
}

