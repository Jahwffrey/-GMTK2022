using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class OnePlayerTransitions : MonoBehaviour
{
    public Camera MainCamera;
    protected Vector3 CameraOrigPosition;

    public UnitController UnitController;
    public PlayerControl Player1Control;
    public PlayerControl Player2Control;
    protected bool WaitingForFirstUpdate = true;
    protected bool DecidedUnits = false;

    protected int StartingUnits = 3;
    protected int StartingDice = 5;


    protected List<PlayerControl.UnitID> PlayerUnitIds;
    protected List<Dice> PlayerDice;

    protected List<PlayerControl.UnitID> EnemyUnitIds;
    protected List<Dice> EnemyDice;


    public GameObject AnnouncementObj;
    public Text AnnouncementText;
    protected int level = 0;

    private void Start()
    {
        CameraOrigPosition = MainCamera.transform.position;
    }

    protected bool NextPregameSetup = false;

    public void SetupGame()
    {
        if (!DecidedUnits)
        {
            DecidedUnits = true;
            PlayerUnitIds = new List<PlayerControl.UnitID>();
            PlayerDice = new List<Dice>();
            EnemyUnitIds = new List<PlayerControl.UnitID>();
            EnemyDice = new List<Dice>();
            var allUnits = System.Enum.GetValues(typeof(PlayerControl.UnitID)).Cast<PlayerControl.UnitID>().ToList();
            var allDice = UnitController.GetAllDice();
            for (int i = 0; i < StartingUnits; i++)
            {
                PlayerUnitIds.Add(allUnits[Random.Range(0, allUnits.Count - 1)]); // -1 so not NONE
                EnemyUnitIds.Add(allUnits[Random.Range(0, allUnits.Count - 1)]); // -1 so not NONE
            }
            for (int i = 0; i < StartingDice; i++)
            {
                PlayerDice.Add(allDice[Random.Range(0, allDice.Count)]);
                EnemyDice.Add(allDice[Random.Range(0, allDice.Count)]);
            }
        }

        Player1Control.SetInventories(PlayerUnitIds, PlayerDice);
        MainCamera.transform.position = CameraOrigPosition;
        MainCamera.transform.LookAt(Vector3.zero);
    }

    protected void ShowAnnouncement(string text)
    {
        AnnouncementObj.SetActive(true);
        AnnouncementText.text = text;
    }

    public void HideAnnouncement()
    {
        AnnouncementObj.SetActive(false);

        if (NextPregameSetup)
        {
            NextPregameSetup = false;
        }
    }


    private void Update()
    {
        if (WaitingForFirstUpdate)
        {
            WaitingForFirstUpdate = false;
            BeginNewGame();
        }
    }

    public void BeginNewGame()
    {
        UnitController.PregameSetup();
    }

    public void StartPlayerSetup()
    {
        level += 1;
        NextPregameSetup = true;
        ShowAnnouncement($"Level {level}");
    }

    public void GameSetupFinished()
    {
        UnitController.StartGame();
    }


    public void GameFinished(UnitController.Winner winner)
    {
    }
}