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

    private bool K1;
    private bool K2;
    private bool K3;

    public bool FollowPlayers { get; set; } = true;

    public int PlayerCount => players.Count;

    // Start is called before the first frame update
    void Start()
    {
        players = new List<Player>();

        manager = GetComponent<PlayerInputManager>();
        manager.JoinPlayer(PlayerCount + 1, PlayerCount + 1, "Keyboard 1", pairWithDevice: Keyboard.current);
        manager.JoinPlayer(PlayerCount + 1, PlayerCount + 1, "Keyboard 2", pairWithDevice: Keyboard.current);
    }

    private void Update()
    {
        if (!FollowPlayers)
            return;

        Vector3 pos = Vector3.zero;
        foreach (Player player in players)
            pos += player.transform.position;
        pos /= PlayerCount;
        pos -= Vector3.forward * 10f;

        pos.y = mainCamera.transform.position.y;

        mainCamera.transform.position = pos;
    }

    public void AddPlayer(PlayerInput player)
    {
        players.Add(player.GetComponent<Player>());
    }

    public void PlayerLeft(PlayerInput player)
    {
        players.Remove(player.GetComponent<Player>());
    }
}