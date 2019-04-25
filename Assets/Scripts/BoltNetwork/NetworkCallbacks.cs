using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[BoltGlobalBehaviour] //Le script ne peut pas etre mis sur un GO mais est appeler par Bolt automatiquement.
public class NetworkCallbacks : Bolt.GlobalEventListener
{
    List<string> logMessages = new List<string>();

    public static int team = 1;
    public static int maxPlayers = 20;

    public override void SceneLoadLocalDone(string map)
    {
        // randomize a position
        var spawnPosition = new Vector3(Random.Range(-8, 8), 0, Random.Range(-8, 8));

        // instantiate cube
        BoltEntity go = BoltNetwork.Instantiate(BoltPrefabs.Guardian, spawnPosition, Quaternion.identity);
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
