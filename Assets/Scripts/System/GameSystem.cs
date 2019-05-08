using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bolt;
using Bolt.Samples.Photon.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSystem : Bolt.EntityEventListener<IGameSystemeState>
{
    public bool SmashSystem = true;
    public static GameSystem GSystem;
    [Header("Party Info")]
    [SerializeField] private float partyTimer = 180.0f;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private TextMeshProUGUI playerNamePrefab;
    private List<TextMeshProUGUI> playerNameList = new List<TextMeshProUGUI>();
    [SerializeField] private Guardian guardianAssignWorlCanvas;
    [field:SerializeField] public int CurrentGuardianInLife { get; private set; }

    [Header("Score")]
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private List<GameObject> playersScore;
    [SerializeField] private GameObject prefabPlayersScore;
    [SerializeField] private float timeSortScore = 1f;

    [Header("End Game")]
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private GameObject cameraFinal;
    public bool EndGame = false;

    [Header("Kill Feed")]
    [SerializeField] private List<GameObject> killFeedList = new List<GameObject>();
    [SerializeField] private int maxKillFeedPref = 5;
    [SerializeField] private GameObject killFeedPanel;
    [SerializeField] private GameObject killFeedPrefab;

    public List<Guardian> GuardiansInScene;
    private List<Guardian> GuardianSortByScore;
    

    private void Awake()
    {
        CurrentGuardianInLife = 99;
        GameSystem.GSystem = this;
        StartCoroutine(WaitToFindGuardians());
        EndGame = false;
        
    }

    private void Update()
    {
        timerText.text = string.Format("{0:0}:{1:00}", Mathf.Floor(partyTimer / 60), partyTimer % 60 > 59 ? 59 : partyTimer % 60);

        if (partyTimer > 0.01f)
        {
            if (playersScore.Count > 0)
            {
                AssignScore();
            }

            partyTimer -= Time.deltaTime;

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
                
                this.cameraFinal.SetActive(true);

                if (winnerPanel.activeSelf == false)
                {
                    winnerPanel.SetActive(true);
                    this.scorePanel.SetActive(true);
                }

                winnerText.text = "Winner is " + WinGuardian().guardianName;

                StartCoroutine(Deconnect());

                Debug.Log("Party Fini");
                EndGame = true;
            }
            
        }

        if (CurrentGuardianInLife <= 1)
        {
            this.partyTimer = 0.0f;
        }
    }

    private void AssignScore()
    {
        if (this.partyTimer > 0.2f && CurrentGuardianInLife > 1)
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
                        playerNameList[i].text = GuardiansInScene[i].guardianName;
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
        yield return new WaitForSeconds(3f);
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

        CurrentGuardianInLife = GuardiansInScene.Count;
        SortGuardianByScore();
        yield break;
    }

    private IEnumerator Deconnect()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        yield return new WaitForSeconds(8f);
        var evnt = DisconnectEvent.Create(GlobalTargets.Everyone);
        evnt.Send();
        yield break;
    }

    public Guardian BestEnemyGuardian(Guardian mySelf)
    {
        Guardian enemy = null;
        int score = -9999;

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
        }

        return enemy != null ? enemy : null;
    }

    private Guardian WinGuardian()
    {
        Guardian win = null;
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

        return win != null ? win : GuardiansInScene[Random.Range(0, GuardiansInScene.Count)];
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

            int scoreMin = 999999;
            Guardian g = null;

            for (int i = 0; i < GuardianSortByScore.Count; i++)
            {
                foreach (var gScore in GuardianSortByScore)
                {
                    if (gScore.CurrentScore < scoreMin)
                    {
                        if (!gList.Contains(gScore))
                        {
                            scoreMin = gScore.CurrentScore;
                            g = gScore;
                        }

                    }
                }

                gList.Add(g);

                g = null;
                scoreMin = 999999;
            }

            gList.Reverse();
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

    public void GuardianDie()
    {
        CurrentGuardianInLife--;
        this.cameraFinal.SetActive(true);
    }
}
