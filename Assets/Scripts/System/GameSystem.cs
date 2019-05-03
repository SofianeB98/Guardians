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
    public static GameSystem GSystem;
    [Header("Timer Party")]
    [SerializeField] private float partyTimer = 180.0f;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Score")]
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private List<GameObject> playersScore;
    [SerializeField] private GameObject prefabPlayersScore;

    [Header("Winner")]
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private GameObject winnerPanel;

    [Header("Kill Feed")]
    [SerializeField] private List<GameObject> killFeedList = new List<GameObject>();
    [SerializeField] private int maxKillFeedPref = 5;
    [SerializeField] private GameObject killFeedPanel;
    [SerializeField] private GameObject killFeedPrefab;

    public List<Guardian> GuardiansInScene;
    public bool EndGame = false;

    private void Awake()
    {
        GameSystem.GSystem = this;
        StartCoroutine(WaitToFindGuardians());
        EndGame = false;
    }

    private void Update()
    {
        if (playersScore.Count > 0)
        {
            AssignScore();
        }
        
        timerText.text = string.Format("{0:0}:{1:00}", Mathf.Floor(partyTimer / 60), partyTimer % 60 > 59 ? 59 : partyTimer % 60);

        if (partyTimer > 0.0f)
        {
            partyTimer -= Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                this.scorePanel.SetActive(!this.scorePanel.activeSelf);
            }
        }
        else
        {
            partyTimer = 0.0f;
            if (!EndGame)
            {
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

        
    }

    private void AssignScore()
    {
        if (playersScore.Count > 0)
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
                            texts[j].text = GuardiansInScene[i].guardianName;
                        }
                        else if (texts[j].name.Contains("Score"))
                        {
                            texts[j].text = GuardiansInScene[i].CurrentScore.ToString();
                        }
                        else if (texts[j].name.Contains("Kill"))
                        {
                            texts[j].text = GuardiansInScene[i].CurrentKill.ToString();
                        }
                    }
                }
                
            }
        }
    }

    public void AddPlayersScore()
    {
        GameObject go = Instantiate(prefabPlayersScore, scorePanel.transform);
        playersScore.Add(go);
        
        
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
        yield return new WaitForSeconds(2f);
        GuardiansInScene = FindObjectsOfType<Guardian>().ToList();
        if (GuardiansInScene.Count > 0)
        {
            foreach (var VARIABLE in GuardiansInScene)
            {
                yield return new WaitForEndOfFrame();
                AddPlayersScore();
            }
        }
        yield break;
    }

    private IEnumerator Deconnect()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        yield return new WaitForSeconds(10f);
        if (BoltNetwork.IsServer)
        {
            var evnt = DisconnectEvent.Create(GlobalTargets.Everyone);
            evnt.Send();
        }
        
        yield break;
    }

    private Guardian WinGuardian()
    {
        Guardian win = null;
        int score = 0;

        for (int i = 0; i < this.GuardiansInScene.Count; i++)
        {
            if (GuardiansInScene[i].CurrentScore > score)
            {
                win = GuardiansInScene[i];
                score = GuardiansInScene[i].CurrentScore;
            }
        }

        return win != null ? win : GuardiansInScene[Random.Range(0, GuardiansInScene.Count)];
    }

}
