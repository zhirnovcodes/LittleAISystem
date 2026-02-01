using LittleAI.Enums;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public abstract class ActionMapBase : MonoBehaviour
{
    public abstract List<ActionsMapItem> GetActionsMapList();
    public abstract Dictionary<SubActionTypes, ISubActionState> ConstructSubActionsStates(SystemBase system);
}
