using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public static class DNAExtensions
{
    /// <summary>
    /// Checks if two DNA chains are compatible by verifying that each item is present in both chains
    /// (checking by GenomeType and GenomeData.Index)
    /// </summary>
    public static bool IsCompatible(DynamicBuffer<DNAChainItem> father, DynamicBuffer<DNAChainItem> mother)
    {
        // Both must have the same number of genes
        if (father.Length != mother.Length)
            return false;
        
        // Check each father gene exists in mother
        for (int i = 0; i < father.Length; i++)
        {
            bool found = false;
            var fatherGene = father[i].Data;
            
            for (int j = 0; j < mother.Length; j++)
            {
                var motherGene = mother[j].Data;
                
                if (fatherGene.GenomeType == motherGene.GenomeType && 
                    fatherGene.GenomeData.Index == motherGene.GenomeData.Index)
                {
                    found = true;
                    break;
                }
            }
            
            if (!found)
                return false;
        }
        
        // Check each mother gene exists in father
        for (int i = 0; i < mother.Length; i++)
        {
            bool found = false;
            var motherGene = mother[i].Data;
            
            for (int j = 0; j < father.Length; j++)
            {
                var fatherGene = father[j].Data;
                
                if (motherGene.GenomeType == fatherGene.GenomeType && 
                    motherGene.GenomeData.Index == fatherGene.GenomeData.Index)
                {
                    found = true;
                    break;
                }
            }
            
            if (!found)
                return false;
        }
        
        return true;
    }
    
    /// <summary>
    /// Gets a random father entity from the DNA storage buffer
    /// </summary>
    public static Entity GetRandomFather(DynamicBuffer<DNAStorageItem> storage, ref Random random)
    {
        if (storage.Length == 0)
            return Entity.Null;
        
        int randomIndex = random.NextInt(0, storage.Length);
        return storage[randomIndex].Father;
    }
    
    /// <summary>
    /// Extracts DNA chain data from a specific father in the storage and adds it to the result list
    /// </summary>
    public static void GetFatherDNA(DynamicBuffer<DNAStorageItem> storage, Entity father, ref NativeList<DNAChainData> result)
    {
        for (int i = 0; i < storage.Length; i++)
        {
            if (storage[i].Father == father)
            {
                result.Add(storage[i].Data);
            }
        }
    }
    
    /// <summary>
    /// Converts a DNA chain buffer to a NativeList of DNAChainData
    /// </summary>
    public static void ToList(DynamicBuffer<DNAChainItem> buffer, ref NativeList<DNAChainData> result)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            result.Add(buffer[i].Data);
        }
    }
    
    /// <summary>
    /// Creates offspring DNA by lerping between father and mother DNA chains
    /// For each gene, randomly chooses from which parent to inherit
    /// </summary>
    public static void Lerp(NativeList<DNAChainData> father, NativeList<DNAChainData> mother, ref NativeList<DNAChainData> result, ref Random random)
    {
        // Clear result list
        result.Clear();
        
        // For each gene in the father DNA (assuming compatible DNA chains)
        for (int i = 0; i < father.Length; i++)
        {
            var fatherGene = father[i];
            
            // Find matching gene in mother DNA
            DNAChainData motherGene = default;
            bool foundMatch = false;
            
            for (int j = 0; j < mother.Length; j++)
            {
                if (mother[j].GenomeType == fatherGene.GenomeType && 
                    mother[j].GenomeData.Index == fatherGene.GenomeData.Index)
                {
                    motherGene = mother[j];
                    foundMatch = true;
                    break;
                }
            }
            
            if (!foundMatch)
            {
                // If no match found, just use father's gene
                result.Add(fatherGene);
                continue;
            }
            
            // Create offspring gene by lerping the data
            DNAChainData offspringGene = new DNAChainData
            {
                GenomeType = fatherGene.GenomeType,
                GenomeData = new GenomeData
                {
                    Index = fatherGene.GenomeData.Index,
                    Data = math.lerp(fatherGene.GenomeData.Data, motherGene.GenomeData.Data, random.NextFloat())
                }
            };
            
            result.Add(offspringGene);
        }
    }
}

