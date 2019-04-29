using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameSystem : Bolt.EntityEventListener<IGameSystemeState>
{
    public static GameSystem GSystem;

    [SerializeField] private float partyTimer = 180.0f;
    [SerializeField] private TextMeshProUGUI timerText;

    [SerializeField] private GameObject scorePanel;
    [SerializeField] private List<GameObject> playersScore;
    [SerializeField] private GameObject prefabPlayersScore;

    [SerializeField] private TextMeshProUGUI winnerText;

    public List<Guardian> GuardiansInScene;

    private void Awake()
    {
        GameSystem.GSystem = this;
        StartCoroutine(WaitToFindGuardians());
    }

    private void Update()
    {
        if (playersScore.Count > 0)
        {
            AssignScore();
        }
        
        timerText.text = string.Format("{0:0}:{1:00}", Mathf.Floor(partyTimer / 60), partyTimer % 60);

        if (partyTimer > 0.0f)
        {
            partyTimer -= Time.deltaTime;
        }
        else
        {
            partyTimer = 0.0f;

            if (winnerText.enabled == false)
            {
                winnerText.enabled = true;
            }

            winnerText.text = "Winner is " + WinGuardian().guardianName;

            StartCoroutine(Deconnect());

            Debug.Log("Party Fini");
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            this.scorePanel.SetActive(!this.scorePanel.activeSelf);
        }
    }

    private void AssignScore()
    {
        if (playersScore.Count > 0)
        {
            for (int i = 0; i < playersScore.Count; i++)
            {
                TextMeshProUGUI[] texts = playersScore[i].GetComponentsInChildren<TextMeshProUGUI>();
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

    public void AddPlayersScore()
    {
        GameObject go = Instantiate(prefabPlayersScore, scorePanel.transform);
        playersScore.Add(go);
        
        
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
        yield return new WaitForSeconds(4f);
        BoltNetwork.Shutdown();
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
