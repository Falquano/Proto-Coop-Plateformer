using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrownSpawnerManager : MonoBehaviour
{
    [SerializeField] List<Spawner> spawners;
    [SerializeField] Spawner lastspawn;
    [SerializeField] float spawnableRadius;

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    void Update()
    {
        foreach (Spawner s in spawners)
        {
            if(!s.HasReachedMaxLocalInstances())
                lastspawn = s;

            if (Physics2D.OverlapCircle(s.transform.position, spawnableRadius))
            {
                s.activated = true;
            }
            else
            {
                s.activated = false;
            }

            if (s == lastspawn)
                s.activated = false;
        }

    }
}
