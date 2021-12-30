using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Dynamic;

public class SpawnerSharedInstancesReferer : MonoBehaviour
{
    [Header("Instances")]
    [SerializeField] bool useMaxInstances = true;
    [SerializeField] int maximumInstancesAtOnce = 1;
    int currentInstancesAtOnce = 0;

    [Header("Spawner Shared Instances Referer")]
    [SerializeField] bool sharedMaxInstances = false;
    [SerializeField] SpawnerSharedInstancesReferer sharedMaxInstancesReferer;

    [Header("Debug")]
    [SerializeField] bool DebugHasReachedMaximumSpawn;
    [SerializeField] int DebugCurrentInstancesAtOnce = 0;


    public bool CanAddInstance()
    {
        bool CanAdd = true;
        if (useMaxInstances && maximumInstancesAtOnce <= currentInstancesAtOnce)
            CanAdd = false;
        if (sharedMaxInstances && !sharedMaxInstancesReferer.CanAddInstance())
            CanAdd = false;
        return CanAdd;
    }

    public void AddInstance()
    {
        currentInstancesAtOnce++;
        if (!IsALoop() && sharedMaxInstances)
            sharedMaxInstancesReferer.AddInstance();


        UpdateDebug();
    }

    public void RemoveInstance()
    {
        currentInstancesAtOnce--;
        if (!IsALoop() && sharedMaxInstances)
            sharedMaxInstancesReferer.RemoveInstance();
        //If there is a negative when removing, go to 0 to avoid potential problems when using the value
        if (currentInstancesAtOnce < 0)
            currentInstancesAtOnce = 0;

        UpdateDebug();
    }

    bool IsALoop()
    {
        // Debug.Log(this);
        // Debug.Log(sharedMaxInstancesReferer);
        // Debug.Log(this == sharedMaxInstancesReferer);

        return (this == sharedMaxInstancesReferer);
    }


    void UpdateDebug()
    {
        //Debug
        DebugHasReachedMaximumSpawn = (currentInstancesAtOnce==maximumInstancesAtOnce);
        DebugCurrentInstancesAtOnce = currentInstancesAtOnce;
    }
}
