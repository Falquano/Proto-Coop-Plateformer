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

    public int Count => players.Count;

    // COULEURS
    private List<float> availableHues;


    void Start()
    {
        players = new List<Player>();

        manager = GetComponent<PlayerInputManager>();

        availableHues = CreateHueList();

        manager.JoinPlayer(Count + 1, Count + 1, "Keyboard 1", pairWithDevice: Keyboard.current);
        manager.JoinPlayer(Count + 1, Count + 1, "Keyboard 2", pairWithDevice: Keyboard.current);
        
        manager.EnableJoining();
    }

    public List<float> CreateHueList()
    {
        List<float> hues = new List<float>();
        int length = manager.maxPlayerCount;

        float offset = Random.Range(0f, 1f / (float)length);

        for (int i = 0; i < length; i++)
            hues.Add(1f / (float)length * (float)i + offset);

        return hues;
    }

    public float RequestHue()
    {
        if (availableHues.Count <= 0)
            throw new System.Exception("No more available hues !");

        float hue = GetRandomAvailableHue();
        availableHues.Remove(hue);
        return hue;
    }

    private float GetRandomAvailableHue()
    {
        return availableHues[Random.Range(0, availableHues.Count)];
    }

    public void AddAvailableHue(float hue)
    {
        availableHues.Add(hue);
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        players.Add(playerInput.GetComponent<Player>());
    }

    public void OnPlayerLeft(PlayerInput playerInput)
    {
        Debug.LogWarning("Player left");
        players.Remove(playerInput.GetComponent<Player>());
        Destroy(playerInput.gameObject);
    }
}
