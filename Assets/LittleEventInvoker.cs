using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LittleEventInvoker : MonoBehaviour
{
    public UnityEvent Event { get; }

    public LittleEventInvoker()
    {
        Event = new UnityEvent();
    }

    public void Invoke()
    {
        Event.Invoke();
    }
}
