using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class AnimalStatsAttenuationAuthoring : MonoBehaviour
{
    [HermiteCurveNormalized] public HermiteCurve h0;

    [Header("Needs Attenuation")]
    [SerializeField] private AnimationCurve Energy = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private AnimationCurve Fullness = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private AnimationCurve Toilet = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private AnimationCurve Social = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private AnimationCurve Safety = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private AnimationCurve Health = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Distance Attenuation")]
    [SerializeField] private AnimationCurve EnergyDistance = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private AnimationCurve FullnessDistance = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private AnimationCurve ToiletDistance = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private AnimationCurve SocialDistance = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private AnimationCurve SafetyDistance = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private AnimationCurve HealthDistance = AnimationCurve.Linear(0, 0, 1, 1);

    class Baker : Baker<AnimalStatsAttenuationAuthoring>
    {
        public override void Bake(AnimalStatsAttenuationAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // Convert AnimationCurves to HermiteCurves
            var energyCurve = HermiteCurveExtension.ConvertFromAnimationCurve(authoring.Energy);
            var fullnessCurve = HermiteCurveExtension.ConvertFromAnimationCurve(authoring.Fullness);
            var toiletCurve = HermiteCurveExtension.ConvertFromAnimationCurve(authoring.Toilet);
            var socialCurve = HermiteCurveExtension.ConvertFromAnimationCurve(authoring.Social);
            var safetyCurve = HermiteCurveExtension.ConvertFromAnimationCurve(authoring.Safety);
            var healthCurve = HermiteCurveExtension.ConvertFromAnimationCurve(authoring.Health);

            var energyDistanceCurve = HermiteCurveExtension.ConvertFromAnimationCurve(authoring.EnergyDistance);
            var fullnessDistanceCurve = HermiteCurveExtension.ConvertFromAnimationCurve(authoring.FullnessDistance);
            var toiletDistanceCurve = HermiteCurveExtension.ConvertFromAnimationCurve(authoring.ToiletDistance);
            var socialDistanceCurve = HermiteCurveExtension.ConvertFromAnimationCurve(authoring.SocialDistance);
            var safetyDistanceCurve = HermiteCurveExtension.ConvertFromAnimationCurve(authoring.SafetyDistance);
            var healthDistanceCurve = HermiteCurveExtension.ConvertFromAnimationCurve(authoring.HealthDistance);

            // Build the attenuation
            var attenuationBuilder = new AnimalStatsAttenuationBuilder();
            
            var attenuation = attenuationBuilder
                .WithEnergy(energyCurve, energyDistanceCurve)
                .WithFullness(fullnessCurve, fullnessDistanceCurve)
                .WithToilet(toiletCurve, toiletDistanceCurve)
                .WithSocial(socialCurve, socialDistanceCurve)
                .WithSafety(safetyCurve, safetyDistanceCurve)
                .WithHealth(healthCurve, healthDistanceCurve)
                .Build();

            // Add the component
            AddComponent(entity, new AnimalStatsAttenuationComponent
            {
                Attenuation = attenuation
            });
        }
    }
}

