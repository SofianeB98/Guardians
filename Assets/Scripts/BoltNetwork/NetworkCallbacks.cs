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
        
        UpdateTeam();
    }

    void UpdateTeam()
    {
        if (team < 20)
        {
            team++;

            int index = team < SpawnPointsTransforms.Length ? team : team - SpawnPointsTransforms.Length;
            index = index < SpawnPointsTransforms.Length ? index : index - SpawnPointsTransforms.Length;
            index = index < SpawnPointsTransforms.Length ? index : index - SpawnPointsTransforms.Length;
            index = index < SpawnPointsTransforms.Length ? index : index - SpawnPointsTransforms.Length;

            var spawnPosition = SpawnPointsTransforms[index].transform.position + Vector3.up * 2;

            // instantiate guardian
            BoltEntity go = BoltNetwork.Instantiate(BoltPrefabs.Guardian, spawnPosition, Quaternion.identity);
        }
    }

    public override void OnEvent(LogEvent evnt)
    {
        logMessages.Insert(0, evnt.Message);
    }

    public override void OnEvent(DisconnectEvent evnt)
    {
        //Camera[] cam = FindObjectsOfType<Camera>();
        //foreach (var VARIABLE in cam)
        //{
         //   BoltNetwork.Destroy(VARIABLE.gameObject);
        //}

        BoltNetwork.Shutdown();
        
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    void DisconnectAll()
    {
        foreach (var connection in BoltNetwork.Connections)
        {
            connection.Disconnect();
        }
    }

    public override void Disconnected(BoltConnection connection)
    {
        LobbyPhotonPlayer player = connection.GetLobbyPlayer();
        if (player != null)
        {
            BoltLog.Info("Disconnected");
            player.RemovePlayer();
        }
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
