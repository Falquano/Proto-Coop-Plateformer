using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoLevel : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private PatternProvider patternProvider;

    [SerializeField] private Transform grid;

    private ExitFlags ComesFrom;
    private ExitFlags GoesTo;

    [SerializeField] private Pattern currentPattern;

	private void Start()
	{
        NewGame();
	}

	public void NewGame()
	{
        ComesFrom = ExitFlags.None;
        GoesTo = ExitFlag.RandomExitDirection();
        currentPattern.NewRound(GoesTo);
	}

    public void NewRound()
	{
        Debug.Log("New Round !");

        ComesFrom = ExitFlag.Opposite(GoesTo);

        SwitchToNewPattern();

        playerManager.NewRound();
    }

    private void SwitchToNewPattern()
	{
        Destroy(currentPattern.gameObject);

        GoesTo = ExitFlag.RandomExitDirection(ComesFrom);

        currentPattern = Instantiate(SelectPattern(GoesTo), grid).GetComponent<Pattern>();
        currentPattern.NewRound(GoesTo);

	}

    private GameObject SelectPattern(ExitFlags entrance)
	{
        return patternProvider.Find(entrance);
	}

    public Exit Entrance() 
    {
        foreach (Exit exit in currentPattern.Exits)
		{
            if (exit.Direction == ComesFrom)
                return exit;
		}
        throw new System.Exception("Oskour il n'y a pas d'entrée !");
    }

    public Exit Exit()
    {
        foreach (Exit exit in currentPattern.Exits)
        {
            if (exit.Direction == ComesFrom)
                return exit;
        }
        throw new System.Exception("Oskour il n'y a pas de sortie !");
    }
}
