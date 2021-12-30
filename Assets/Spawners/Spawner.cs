using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Dynamic;

public class Spawner : MonoBehaviour
{
    [Header("Spawner")]
    public bool activated = true;
    [SerializeField] float frequencyOfSpawnInSeconds = 0;
    [SerializeField] bool skipFirstSpawn = false;
    float currentTimeUntilSpawn = 0f;

    [Header("Instances")]
    [SerializeField] bool useMaxInstances = true;
    [SerializeField] int maximumInstancesAtOnce = 1;
    int currentInstancesAtOnce = 0;

    [Header("Object")]
    [SerializeField] GameObject toSpawn;
    [SerializeField] Vector3 spawnOffset;

    [Header("Spawner Shared Instances Referer")]
    [SerializeField] bool sharedMaxInstances = false;
    [SerializeField] SpawnerSharedInstancesReferer sharedMaxInstancesReferer;

    [Header("Per instance")]
    [SerializeField] float timeBeforeActivation = 0f;

    [Header("Debug")]
    [SerializeField] bool Debug_HasReachedMaximumSpawn;
    [SerializeField] int Debug_CurrentInstancesAtOnce = 0;


    // Start is called before the first frame update
    void Start()
    {
        if(skipFirstSpawn)
            currentTimeUntilSpawn = frequencyOfSpawnInSeconds;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTimeUntilSpawn > 0)
            currentTimeUntilSpawn -= Time.deltaTime;


        if ((activated && currentTimeUntilSpawn <= 0) && ((!useMaxInstances) || (useMaxInstances && (maximumInstancesAtOnce > currentInstancesAtOnce))))
        {
            if ((!sharedMaxInstances) || (sharedMaxInstances && sharedMaxInstancesReferer.CanAddInstance()))
            {
                Spawn();
            }
        }

        //Debug
        Debug_HasReachedMaximumSpawn = (currentInstancesAtOnce==maximumInstancesAtOnce);
        Debug_CurrentInstancesAtOnce = currentInstancesAtOnce;
        // Debug.Log(currentInstancesAtOnce);
    }

    void Spawn()
    {
        currentTimeUntilSpawn = frequencyOfSpawnInSeconds;

        GameObject newInstance = Instantiate(toSpawn, transform.position + spawnOffset, transform.rotation, transform);
        var spawnParams = newInstance.AddComponent<SpawnParameters>();
        // newInstance.GetComponent<SpawnParameters>().spawner = this;
        spawnParams.OnRemoveInstance.AddListener(RemoveInstance);
        spawnParams.timeBeforeActivation = timeBeforeActivation;


        if (sharedMaxInstances)
            sharedMaxInstancesReferer.AddInstance();
        
        currentInstancesAtOnce++;
    }

    public void RemoveInstance()
    {
        currentInstancesAtOnce--;
        if (sharedMaxInstances)
            sharedMaxInstancesReferer.RemoveInstance();
    }
}