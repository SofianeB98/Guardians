using System.Collections;
using System.Collections.Generic;
using Bolt.Samples.Photon.Lobby;
using UnityEngine;
using UnityEngine.SceneManagement;

[BoltGlobalBehaviour("Multiplayer")] //Le script ne peut pas etre mis sur un GO mais est appeler par Bolt automatiquement.
public class NetworkCallbacks : Bolt.GlobalEventListener
{
    List<string> logMessages = new List<string>();

    public static int team = 1;
    public static int maxPlayers = 20;

    public static GameObject[] SpawnPointsTransforms;

    public List<Guardian> GuardiansInScene;

    public override void SceneLoadLocalDone(string map)
    {
        SpawnPointsTransforms = GameObject.FindGameObjectsWithTag("Spawn");

        // randomize a position
        var spawnPosition = SpawnPointsTransforms[Random.Range(0, SpawnPointsTransforms.Length)].transform.position + Vector3.up * 2;

        // instantiate guardian
        BoltEntity go = BoltNetwork.Instantiate(BoltPrefabs.Guardian, spawnPosition , Quaternion.identity);
        UpdateTeam();
    }

    void UpdateTeam()
    {
        if (team < 20)
        {
            team++;
        }
    }

    public override void OnEvent(LogEvent evnt)
    {
        logMessages.Insert(0, evnt.Message);
    }

    public override void OnEvent(DisconnectEvent evnt)
    {
        foreach (var connect in BoltNetwork.Connections)
        {
            LobbyManager.s_Singleton.Disconnected(connect);
            BoltNetwork.Shutdown();
        }

        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    /*void OnGUI()
    {
        // only display max the 5 latest log messages
        int maxMessages = Mathf.Min(5, logMessages.Count);

        GUILayout.BeginArea(new Rect(Screen.width / 2 - 200, Screen.height - 100, 400, 100), GUI.skin.box);

        for (int i = 0; i < maxMessages; ++i)
        {
            GUILayout.Label(logMessages[i]);
        }

        GUILayout.EndArea();
    }*/
}
