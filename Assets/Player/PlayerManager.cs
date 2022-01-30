using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private PlayerInputManager manager;
    private List<Player> players;
    private Player king;
    public Player[] Players => players.ToArray();

    public List<Player> Winners = new List<Player>();
    [SerializeField] private int winnersThisRound = 1;
    [SerializeField] private AutoLevel autoLevel;

    public int Count => players.Count;

    // COULEURS
    [SerializeField] private List<Color> availableHues;
    [SerializeField] private bool dynamicHues = false;
    [SerializeField] private float dynamicSaturation = .75f;
    [SerializeField] private float dynamicValue = .75f;


    void Start()
    {
        players = new List<Player>();

        manager = GetComponent<PlayerInputManager>();

        if (dynamicHues) 
            availableHues = CreateHueList();

        manager.JoinPlayer(Count + 1, Count + 1, "Keyboard 1", pairWithDevice: Keyboard.current);
        manager.JoinPlayer(Count + 1, Count + 1, "Keyboard 2", pairWithDevice: Keyboard.current);
        
        manager.EnableJoining();
    }

    public List<Color> CreateHueList()
    {
        List<Color> hues = new List<Color>();
        int length = manager.maxPlayerCount;

        float offset = Random.Range(0f, 1f / (float)length);

        for (int i = 0; i < length; i++)
            hues.Add(Color.HSVToRGB(1f / (float)length * (float)i + offset, 
                dynamicSaturation, dynamicValue));

        return hues;
    }

    public Color RequestColor()
    {
        if (availableHues.Count <= 0)
            throw new System.Exception("No more available hues !");

        Color hue = GetRandomAvailableHue();
        availableHues.Remove(hue);
        return hue;
    }

    private Color GetRandomAvailableHue()
    {
        return availableHues[Random.Range(0, availableHues.Count)];
    }

    public void AddAvailableHue(Color hue)
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

    public void NewRound()
	{
        // On respawn les gagnants, on supprime le perdant ! (on le met au goulag ?)
        foreach (Player player in Winners)
		{
            RespawnNewRound(player);
		}

        Winners = new List<Player>();
	}

    public void RespawnNewRound(Player player)
	{
        //player.gameObject.SetActive(true);
        player.WakeUp(autoLevel.Entrance().SpawnPoint);
	}

    public void Win(Player player)
	{
        Winners.Add(player);
        //player.gameObject.SetActive(false);
        player.Hybernate();
        
        if (Winners.Count >= winnersThisRound)
		{
            EndRound();
		}
	}

    public void EndRound()
	{
        autoLevel.NewRound();
	}

    public void ShatterCrown()
    {
        if (king == null)
            return;

        king.SetCrown(false);
        king = null;
        // Il faut respawn une nouvelle couronne maintenant !
    }

    public void SetKing(Player player)
    {
        if (king != null)
            ShatterCrown();

        king = player;
        king.SetCrown(true);
    }
}
