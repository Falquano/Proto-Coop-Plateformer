using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoopCameraTracker : MonoBehaviour
{
    [SerializeField] private PlayerManager players;
    [SerializeField] private CoopCameraMode cameraMode;
    public CoopCameraMode CameraMode { get => cameraMode; set => cameraMode = value; }
    [SerializeField] private float zOffset = -10f;

    // UPDATES

    private void Awake()
    {
        if (players == null)
            players = GameObject.Find("@Player Manager").GetComponent<PlayerManager>();
    }

    void Update()
    {
        switch(CameraMode)
        {
            case CoopCameraMode.AverageX:
                UpdateAverageX();
                return;
            case CoopCameraMode.FollowPlayerOne:
                UpdateFollowPlayerOne();
                return;
            default:
                UpdateAverageXY();
                return;
        }
    }

    private void UpdateAverageXY()
    {
        transform.position = new Vector3(GetAveragePlayerPosition().x, GetAveragePlayerPosition().y, zOffset);
    }

    private void UpdateAverageX()
    {
        transform.position = new Vector3(GetAveragePlayerPosition().x, transform.position.y, zOffset);
    }

    private void UpdateFollowPlayerOne()
    {
        // Pas très propre, faudrait pouvoir choisir le joueur. #TODO
        transform.position = new Vector3(players.Players[0].transform.position.x, players.Players[0].transform.position.y, zOffset);
    }

    // REQUETES

    public Vector3 GetAveragePlayerPosition()
    {
        Vector3 average = Vector3.zero;
        foreach(Player player in players.Players)
        {
            average += player.transform.position;
        }
        return average / players.Count;
    }
}

public enum CoopCameraMode
{
    AverageXY,
    AverageX,
    FollowPlayerOne
}
