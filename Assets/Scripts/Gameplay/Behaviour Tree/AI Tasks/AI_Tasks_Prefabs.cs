using System.Collections.Generic;
using UnityEngine;
using Panda;


[RequireComponent(typeof(PandaBehaviour))]
public class AI_Tasks_Prefabs : MonoBehaviour
{
    public List<GameObject> prefabsToActivate = new List<GameObject>();
    public List<GameObject> prefabsToInstance = new List<GameObject>();

    [Task]
    public void ActivateAllPrefabs(bool active)
    {
        for (int i = 0; i < prefabsToInstance.Count; i++)
        {
            prefabsToActivate[i].SetActive(active);
        }

        Task.current.Succeed();
    }

    [Task]
    public void ActivatePrefabAt(int index, bool active)
    {
        prefabsToActivate[index].SetActive(active);
        Task.current.Succeed();
    }

    [Task]
    public void InstanceAllPrefabs()
    {
        for (int i = 0; i < prefabsToInstance.Count; i++)
        {
            GameObject temp = Instantiate(prefabsToInstance[i]) as GameObject;
            temp.transform.position = transform.position;
        }

        Task.current.Succeed();
    }

    [Task]
    public void InstanceAllPrefabsWithParent()
    {
        for (int i = 0; i < prefabsToInstance.Count; i++)
        {
            Instantiate(prefabsToInstance[i], transform.position, Quaternion.identity, transform);
        }
        Task.current.Succeed();
    }

    [Task]
    public void InstanceAllPrefabsWithOffset(float x, float y, float z)
    {
        for (int i = 0; i < prefabsToInstance.Count; i++)
        {
            GameObject temp = Instantiate(prefabsToInstance[i]) as GameObject;
            temp.transform.position = transform.position + new Vector3(x, y, z);
        }

        Task.current.Succeed();
    }

    [Task]
    public void InstancePrefabAt(int index)
    {
          GameObject temp = Instantiate(prefabsToInstance[index]) as GameObject;
          temp.transform.position = transform.position;
          Task.current.Succeed();
    }

}
