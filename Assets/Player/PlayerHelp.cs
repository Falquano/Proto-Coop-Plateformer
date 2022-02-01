using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHelp : MonoBehaviour
{
    [Header("Objet")]
    [Space]
    [SerializeField] private PlayerFXEmitter FX;
    private Player player;
    private BetterCharacterController2D bcc;

    [Header("Values")]
    [Space]
    [SerializeField] private float helpStrength = 20f;
    public float Strength => helpStrength;
    [SerializeField] private float helpLoss = 0.035f;
    public float Loss => helpLoss;
    [SerializeField] private float helpCooldown = .1f;
    [SerializeField] private float baseHelpRadius = 4.2f;
    private float helpRadiusMod = 1f;
    public float helpRadius => baseHelpRadius * helpRadiusMod;
    [SerializeField] private float helpRadiusDecay = 0.33f;
    [SerializeField] private float helpRadiusModRecoveryPerSecond = .015f;
    private bool helpAvailable = true;
    private float helpTime = 10f;
    public bool CanHelp => helpAvailable && helpTime >= helpCooldown;
    public float HelpMod { get; set; } = 1f;
    [SerializeField] private float crownHelpRadiusMultiplier = 1f;
    [SerializeField] private float crownHelpStrengthMultiplier = 1f;
    private void Awake()
    {
        player = GetComponent<Player>();
        bcc = GetComponent<BetterCharacterController2D>();
        if (FX == null)
            FX = GetComponent<PlayerFXEmitter>();

        FX.UpdateHelpScale(helpRadius * ((bcc.isCrowned) ? crownHelpRadiusMultiplier : 1));
    }

    private void Update()
    {
        helpRadiusMod = Mathf.Clamp(helpRadiusMod + helpRadiusModRecoveryPerSecond * Time.deltaTime, 0f, 1f);
        FX.UpdateHelpScale(helpRadius * ((bcc.isCrowned) ? crownHelpRadiusMultiplier : 1)); // Mise � jour de la taille du cercle
        helpTime += Time.deltaTime;
    }

    public void UpdateHelp(PlayerManager manager)
    {
        foreach (Player otherPlayer in manager.Players)
        {
            if (otherPlayer.Equals(player))
                continue;

            if (Vector2.Distance(transform.position, otherPlayer.transform.position) <= helpRadius * ((bcc.isCrowned) ? crownHelpRadiusMultiplier : 1))
            {
                //otherPlayer.PullUp();
                //otherPlayer.HelpMe(this);
                if (HelpMod >= 1f)
                    otherPlayer.PushMe(player, (bcc.isCrowned)?Strength * crownHelpStrengthMultiplier:Strength);
                else
                    otherPlayer.PullMe(player, (bcc.isCrowned)?Strength * crownHelpStrengthMultiplier:Strength);

                player.State = PlayerState.Moving;
                helpTime = 0f;
                helpAvailable = false;
                helpRadiusMod = Mathf.Clamp(helpRadiusMod - helpRadiusDecay, 0f, 1f);
            }
        }
    }

    public void SetAvailable()
    {
        helpAvailable = true;
    }
}
