using System;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ParentDNAAuthoring : MonoBehaviour
{
    public ConditionFlags Flags;
    
    public bool HasReproduction;
    public ReproductionGenomeData Reproduction = new ReproductionGenomeData();
    
    public bool HasSpeed;
    public SpeedGenomeData Speed = new SpeedGenomeData();
    
    public bool HasStatsIncrease;
    public StatsIncreaseGenomeData StatsIncrease = new StatsIncreaseGenomeData();
    
    public bool HasActionChain;
    public ActionChainGenomeData ActionChain = new ActionChainGenomeData();
    
    public bool HasAdvertiser;
    public List<AdvertiserGenomeData> Advertisers = new List<AdvertiserGenomeData>();
    
    public bool HasAging;
    public AgingGenomeData Aging = new AgingGenomeData();
    
    public bool HasNeedsBased;
    public NeedsBasedGenomeData NeedsBased = new NeedsBasedGenomeData();
    
    public bool HasStats;
    public StatsGenomeData Stats = new StatsGenomeData();
    
    public bool HasStatAttenuation;
    public List<StatAttenuationGenomeData> StatAttenuations = new List<StatAttenuationGenomeData>();
    
    public bool HasVision;
    public VisionGenomeData Vision = new VisionGenomeData();
    
    public List<IGenomeDataConvertible> GetGenomes()
    {
        var genomes = new List<IGenomeDataConvertible>();
        
        if (HasReproduction) genomes.Add(Reproduction);
        if (HasSpeed) genomes.Add(Speed);
        if (HasStatsIncrease) genomes.Add(StatsIncrease);
        if (HasActionChain) genomes.Add(ActionChain);
        if (HasAdvertiser)
        {
            foreach (var advertiser in Advertisers)
            {
                genomes.Add(advertiser);
            }
        }
        if (HasAging) genomes.Add(Aging);
        if (HasNeedsBased) genomes.Add(NeedsBased);
        if (HasStats) genomes.Add(Stats);
        if (HasStatAttenuation)
        {
            foreach (var statAttenuation in StatAttenuations)
            {
                genomes.Add(statAttenuation);
            }
        }
        if (HasVision) genomes.Add(Vision);
        
        return genomes;
    }
    
    class Baker : Baker<ParentDNAAuthoring>
    {
        public override void Bake(ParentDNAAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            AddComponent(entity, new ParentDNAComponent
            {
                Flags = authoring.Flags
            });
            
            var dnaBuffer = AddBuffer<DNAChainItem>(entity);
            var genomes = authoring.GetGenomes();
            
            foreach (var genome in genomes)
            {
                dnaBuffer.Add(new DNAChainItem
                {
                    Data = genome.GetDNAData()
                });
            }
        }
    }
}

