using Unity.Entities;
using UnityEngine;

public class TalkingDataComponentAuthoring : MonoBehaviour
{
    public float StumbleFailTime;
    public float MaxDistance;
    public float SocialIncrease;

    class Baker : Baker<TalkingDataComponentAuthoring>
    {
        public override void Bake(TalkingDataComponentAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(entity, new TalkingDataComponent
            {
                StumbleFailTime = authoring.StumbleFailTime,
                MaxDistance = authoring.MaxDistance,
                SocialIncrease = authoring.SocialIncrease
            });
        }
    }
}

