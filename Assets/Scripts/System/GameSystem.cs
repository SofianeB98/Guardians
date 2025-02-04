﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bolt;
using Bolt.Samples.Photon.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSystem : Bolt.EntityEventListener<IGameSystemeState>
{
    //public bool SmashSystem = true;
    public static GameSystem GSystem;
    private float lastSecond = 0f;

    [Header("Party Info")]
    [SerializeField] private float partyTimer = 180.0f;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private TextMeshProUGUI playerNamePrefab;
    private List<TextMeshProUGUI> playerNameList = new List<TextMeshProUGUI>();
    [SerializeField] private Guardian guardianAssignWorlCanvas;
    [field: SerializeField] public bool GameStart = false;
    [SerializeField] private float timerBeforeStartGame = 3f;
    private float currentTimerBeforeStart = 0f;
    [SerializeField] private GameObject compteurPreGamePanel;
    [SerializeField] private TextMeshProUGUI compteurPreGameText;
    //[field:SerializeField] public int CurrentGuardianInLife { get; private set; }

    [Header("Score")]
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private List<GameObject> playersScore;
    [SerializeField] private GameObject prefabPlayersScore;
    [SerializeField] private float timeSortScore = 1f;

    [Header("End Game")]
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private GameObject winnerPanel;
    [field:SerializeField] public GameObject CameraFinal { get; private set; }
    public bool EndGame = false;

    [Header("Kill Feed")]
    [SerializeField] private List<GameObject> killFeedList = new List<GameObject>();
    [SerializeField] private int maxKillFeedPref = 5;
    [SerializeField] private GameObject killFeedPanel;
    [SerializeField] private GameObject killFeedPrefab;

    [Header("Son")]
    [FMODUnity.EventRef]
    [SerializeField] private string rebourtAudioEvent = "";
    [SerializeField] private FMOD.Studio.EventInstance rebourtAudio;

    public List<Guardian> GuardiansInScene;
    private List<Guardian> GuardianSortByScore;
    

    private void Awake()
    {
        //CurrentGuardianInLife = 99;
        GameSystem.GSystem = this;
        StartCoroutine(WaitToFindGuardians());
        EndGame = false;
        GameStart = false;
        this.currentTimerBeforeStart = this.timerBeforeStartGame;
        this.compteurPreGamePanel.SetActive(true);
        this.lastSecond = Time.time + 1f;
        rebourtAudio = FMODUnity.RuntimeManager.CreateInstance(rebourtAudioEvent);
    }

    private void Update()
    {
        timerText.text = string.Format("{0:0}:{1:00}", Mathf.Floor(partyTimer / 60), partyTimer % 60 > 59 ? 59 : partyTimer % 60);

        if (GameStart)
        {
            if (partyTimer > 0.01f)
            {
                if (playersScore.Count > 0)
                {
                    AssignScore();
                }

                partyTimer -= Time.deltaTime;

                if (this.partyTimer > 29.9f && this.partyTimer < 30.0f)
                {
                    rebourtAudio.start();
                }

                if (this.partyTimer > 19.9f && this.partyTimer < 20.0f)
                {
                    rebourtAudio.start();
                }

                if (this.partyTimer <= 10)
                {
                    if (Time.time > this.lastSecond)
                    {
                        rebourtAudio.start();
                        this.lastSecond = Time.time + 1f;
                    }
                }
                else
                {
                    this.lastSecond = Time.time + 1f;
                }

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    this.scorePanel.SetActive(!this.scorePanel.activeSelf);
                }
                else if (Input.GetKeyUp(KeyCode.Tab))
                {
                    this.scorePanel.SetActive(!this.scorePanel.activeSelf);
                }

            }
            else
            {
                partyTimer = 0.0f;
                if (!EndGame)
                {

                    this.CameraFinal.SetActive(true);

                    if (winnerPanel.activeSelf == false)
                    {
                        winnerPanel.SetActive(true);
                        this.scorePanel.SetActive(true);
                    }

                    winnerText.text = "Le vainqueur est \r\n" + WinGuardian().guardianName;

                    StartCoroutine(Deconnect());

                    Debug.Log("Party Fini");
                    EndGame = true;
                }

            }
        }
        else
        {
            //if (BoltNetwork.IsServer)
            {
                if (currentTimerBeforeStart > 0)
                {
                    this.currentTimerBeforeStart -= Time.deltaTime;
                    this.compteurPreGameText.text =
                        "Début de partie dans \r\n" + this.currentTimerBeforeStart.ToString("0");

                    if (this.currentTimerBeforeStart >= 0)
                    {
                        if (Time.time > this.lastSecond)
                        {
                            rebourtAudio.start();
                            this.lastSecond = Time.time + 1f;
                        }
                    }
                }
                else
                {
                    GameStart = true;
                    rebourtAudio.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                    this.compteurPreGamePanel.SetActive(false);
                }
            }
            //else
            {
                //var evnt = DecompteStartEvent.Create(entity, EntityTargets.EveryoneExceptOwner);
               // evnt.CurrentDecompte = this.currentTimerBeforeStart;
               // evnt.GameStart = this.GameStart;
              //  evnt.Send();
            }
        }


        if (Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            this.partyTimer = 999;
        }

        if (Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            this.partyTimer = 5;
        }

        //if (CurrentGuardianInLife <= 1)
        //{
        //    this.partyTimer = 0.0f;
        //}
    }

    private void AssignScore()
    {
        if (this.partyTimer > 0.2f)
        {
            if (playersScore.Count > 0 && GuardianSortByScore.Count > 0)
            {
                for (int i = 0; i < playersScore.Count; i++)
                {
                    TextMeshProUGUI[] texts = playersScore[i].GetComponentsInChildren<TextMeshProUGUI>();
                    if (texts.Length > 0)
                    {
                        for (int j = 0; j < texts.Length; j++)
                        {
                            if (texts[j].name.Contains("Name"))
                            {
                                texts[j].text = GuardianSortByScore[i].guardianName;
                            }
                            else if (texts[j].name.Contains("Score"))
                            {
                                texts[j].text = GuardianSortByScore[i].CurrentScore.ToString();
                            }
                            else if (texts[j].name.Contains("Kill"))
                            {
                                texts[j].text = GuardianSortByScore[i].CurrentKill.ToString();
                            }
                        }
                    }

                    if (GuardiansInScene[i] != this.guardianAssignWorlCanvas)
                    {
                        playerNameList[i].text =
                            GuardiansInScene[i]
                                .guardianName; //+ "\r\n" + GuardiansInScene[i].Life + (GuardiansInScene[i].Life > 1 ? "Vies" : "Vie");
                    }
                    else
                    {
                        playerNameList[i].text = "";
                    }

                    playerNameList[i].transform.position = GuardiansInScene[i].NamePosition.position;
                    if (this.worldCanvas.worldCamera != null) playerNameList[i].transform.rotation =
                        Quaternion.LookRotation((playerNameList[i].transform.position - this.worldCanvas.worldCamera.transform.position), Vector3.up);
                    playerNameList[i].transform.eulerAngles = new Vector3(0, playerNameList[i].transform.eulerAngles.y, 0);
                }
            }
        }
        
    }

    public void AddPlayersScore()
    {
        GameObject go = Instantiate(prefabPlayersScore, scorePanel.transform);
        playersScore.Add(go);

        TextMeshProUGUI name = Instantiate(playerNamePrefab, worldCanvas.transform);
        playerNameList.Add(name);
    }

    public void AddKillFeed(string text)
    {
        if (killFeedList.Count > maxKillFeedPref - 1 )
        {
            GameObject lastFeed = killFeedList[maxKillFeedPref - 1].gameObject;
            killFeedList.RemoveAt(maxKillFeedPref - 1);
            Destroy(lastFeed);
        }

        GameObject go = Instantiate(killFeedPrefab, killFeedPanel.transform);
        go.GetComponentInChildren<TextMeshProUGUI>().text = text;

        killFeedList.Add(go);
    }
    
    public override void OnEvent(KillFeedEvent evnt)
    {
        if (!evnt.RemoveFeed)
        {
            AddKillFeed(evnt.Message);
        }
    }

    private IEnumerator WaitToFindGuardians()
    {
        //yield return new WaitForSeconds(this.timerBeforeStartGame);
        while (!GameStart)
        {
            yield return new WaitForEndOfFrame();
        }

        GuardiansInScene = FindObjectsOfType<Guardian>().ToList();
        GuardianSortByScore = FindObjectsOfType<Guardian>().ToList();

        if (GuardiansInScene.Count > 0)
        {
            foreach (var VARIABLE in GuardiansInScene)
            {
                yield return new WaitForEndOfFrame();
                AddPlayersScore();
            }
        }

        //CurrentGuardianInLife = GuardiansInScene.Count;
        SortGuardianByScore();
        yield break;
    }

    private IEnumerator Deconnect()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        yield return new WaitForSeconds(5f);
        var evnt = DisconnectEvent.Create(GlobalTargets.Everyone);
        evnt.Send();
        yield break;
    }

    public Guardian BestEnemyGuardian(Guardian mySelf)
    {
        /*Guardian enemy = null;
        int score = 0;

        foreach (var guard in GuardiansInScene)
        {
            if (guard != mySelf)
            {
                if (guard.CurrentScore > score)
                {
                    enemy = guard;
                    score = enemy.CurrentScore;
                }
            }
        }*/

        return GuardianSortByScore[0] != mySelf
            ? GuardianSortByScore[0]
            : GuardianSortByScore[GuardianSortByScore.Count > 1 ? 1 : 0]; //enemy != null ? enemy : null;
    }

    private Guardian WinGuardian()
    {

        return GuardianSortByScore[0];
        /*Guardian win = null;
        if (!SmashSystem)
        {
            int score = 0;

            if (GuardiansInScene.Count > 1)
            {
                for (int i = 0; i < this.GuardiansInScene.Count; i++)
                {
                    if (GuardiansInScene[i].CurrentScore >= score)
                    {
                        if (GuardiansInScene[i].CurrentScore == score)
                        {
                            if (GuardiansInScene[i].CurrentKill > win.CurrentKill)
                            {
                                win = GuardiansInScene[i];
                                score = GuardiansInScene[i].CurrentScore;
                            }
                        }

                        win = GuardiansInScene[i];
                        score = GuardiansInScene[i].CurrentScore;
                    }
                }
            }
        }
        else
        {
            foreach (var guard in GuardiansInScene)
            {
                if (guard.Life > 0)
                {
                    win = guard;
                }
            }
        }

        return win != null ? win : GuardiansInScene[Random.Range(0, GuardiansInScene.Count)];*/
    }

    public void AssignCamToWorldCanvas(Camera guardianCam, Guardian g)
    {
        this.worldCanvas.worldCamera = guardianCam;
        this.guardianAssignWorlCanvas = g;
    }

    private void SortGuardianByScore()
    {
        if (GuardianSortByScore.Count > 0)
        {
            List<Guardian> gList = new List<Guardian>();

            int score = 0;
            Guardian g = null;

            for (int i = 0; i < GuardianSortByScore.Count; i++)
            {
                foreach (var gScore in GuardianSortByScore)
                {
                    if (gScore.CurrentScore >= score)
                    {
                        if (!gList.Contains(gScore))
                        {
                            if (g != null)
                            {
                                if (gScore.CurrentScore == score)
                                {
                                    if (gScore.CurrentKill > g.CurrentKill)
                                    {
                                        score = gScore.CurrentScore;
                                        g = gScore;
                                    }
                                }
                                else
                                {
                                    score = gScore.CurrentScore;
                                    g = gScore;
                                }
                            }
                            else
                            {
                                score = gScore.CurrentScore;
                                g = gScore;
                            }


                        }

                    }
                }

                gList.Add(g);

                g = null;
                score = 0;
            }

            //gList.Reverse();
            GuardianSortByScore = gList;
           
        }

        StartCoroutine(WaitToSortScore());
    }
    
    IEnumerator WaitToSortScore()
    {
        yield return new WaitForSeconds(this.timeSortScore);
        this.SortGuardianByScore();
        yield break;
    }

    public override void OnEvent(DecompteStartEvent evnt)
    {
        currentTimerBeforeStart = evnt.CurrentDecompte;
        GameStart = evnt.GameStart;
    }

    //public void GuardianDie()
    //{
       // CurrentGuardianInLife--;
    //}
}
