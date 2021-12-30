using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpawnParameters : MonoBehaviour
{
    // public Spawner spawner;
    bool nonce = true;
    public UnityEvent OnRemoveInstance = new UnityEvent();
    public float timeBeforeActivation = 0f;

    public void RemoveInstance()
    {
        if(nonce)
        {
            OnRemoveInstance.Invoke();
            nonce = false;
        }
        Destroy(this);
    }

    void OnDestroy()
    {
        RemoveInstance();
    }
}
