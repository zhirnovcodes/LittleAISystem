using LittleAI.Enums;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct ActionMapInitializeSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ActionMapInitializeComponent>();
    }

    public void OnDestroy(ref SystemState state)
    {
        if (SystemAPI.HasSingleton<ActionChainConfigComponent>() == false)
        {
            return;
        }

        var component = SystemAPI.GetSingleton<ActionChainConfigComponent>();

        if (component.BlobReference.IsCreated)
        {
            component.BlobReference.Dispose();
        }
    }

    public void OnUpdate(ref SystemState state)
    {
        state.Enabled = false;

        var component = SystemAPI.GetSingleton<ActionMapInitializeComponent>();
        var map = component.Map.Value;

        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

        var entity = commandBuffer.CreateEntity();
        var chainSettings = GetChainSettings(map.GetActionsMapList());
        var configComponent = new ActionChainConfigComponent { BlobReference = chainSettings };

        commandBuffer.AddComponent(entity, configComponent);

        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }

    private static BlobAssetReference<ActionsMapSettings> GetChainSettings(List<ActionsMapItem> managedList)
    {
        var builder = new BlobBuilder(Allocator.Temp);

        ref var root = ref builder.ConstructRoot<ActionsMapSettings>();

        var maxSubActionsCount = 0;
        for (int i = 0; i < managedList.Count; i++)
        {
            maxSubActionsCount = Mathf.Max(managedList[i].SubActions.Count, maxSubActionsCount);
        }

        int actionsCount = System.Enum.GetNames(typeof(ActionTypes)).Length;

        root.ActionsCount = actionsCount;
        root.SubActionsCount = maxSubActionsCount;

        var nodearray = builder.Allocate(ref root.ActionsMap, maxSubActionsCount * actionsCount);

        for (int i = 0; i < managedList.Count; i++)
        {
            for (var j = 0; j < maxSubActionsCount; j++)
            {
                var actionIndex = (int)managedList[i].ActionType;
                var index = actionIndex * maxSubActionsCount + j;
                var subactionIndex = (j < managedList[i].SubActions.Count) ? (int)managedList[i].SubActions[j] : -1;
                nodearray[index] = subactionIndex;
            }
        }

        var result = builder.CreateBlobAssetReference<ActionsMapSettings>(Allocator.Persistent);

        return result;
    }
}
