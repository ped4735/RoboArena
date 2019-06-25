using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class ObjectGroup
{
    [HideLabel]
    public string groupName;
    public List<GameObject> objects;
}


public class NPC_ActivateObjectsHandler : MonoBehaviour
{
    [ListDrawerSettings(ShowIndexLabels = true, ListElementLabelName = "groupName")]
    public List<ObjectGroup> objectGroup;
}
