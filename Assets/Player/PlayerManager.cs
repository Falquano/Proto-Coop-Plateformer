using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private PlayerInputManager manager;
    private List<Player> players;
    public Player[] Players => players.ToArray();

    [SerializeField] 
    private Camera mainCamera;

    public bool FollowPlayers { get; set; } = false;

    public int Count => players.Count;

    void Start()
    {
        players = new List<Player>();

        manager = GetComponent<PlayerInputManager>();
        manager.JoinPlayer(Count + 1, Count + 1, "Keyboard 1", pairWithDevice: Keyboard.current);
        manager.JoinPlayer(Count + 1, Count + 1, "Keyboard 2", pairWithDevice: Keyboard.current);
        
        manager.EnableJoining();
    }

    private void Update()
    {
        if (!FollowPlayers)
            return;

        Vector3 pos = Vector3.zero;
        foreach (Player player in players)
            pos += player.transform.position;
        pos /= Count;
        pos -= Vector3.forward * 10f;

        pos.y = mainCamera.transform.position.y;

        mainCamera.transform.position = pos;
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        players.Add(playerInput.GetComponent<Player>());
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        players.Remove(playerInput.GetComponent<Player>());
    }
}
