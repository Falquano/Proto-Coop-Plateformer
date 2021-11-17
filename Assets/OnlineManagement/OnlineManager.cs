using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Socket.Quobject.SocketIoClientDotNet.Client;
using Newtonsoft.Json;

public class OnlineManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("start");
        var socket = IO.Socket("http://localhost:3000");

        socket.On(QSocket.EVENT_CONNECT, () =>
        {
            Debug.Log("Connected");
            socket.Emit("chat", "test");
        });

        socket.On("chat", data =>{
            Debug.Log(data);
        });
    }

}
