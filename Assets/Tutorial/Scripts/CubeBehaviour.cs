using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeBehaviour : Bolt.EntityEventListener<ICubeState>
{
    public GameObject[] WeaponObjects;
    float resetColorTime;
    Renderer renderer;

    //Correspond a la fonction start
    public override void Attached()
    {
        state.SetTransforms(state.CubeTransform, this.transform);
        if (entity.IsOwner)
        {
            state.CubeColor = new Color(Random.value, Random.value, Random.value);
            // NEW: On the owner, we want to setup the weapons, the Id is set just as the index
            // and the Ammo is randomized between 50 to 100
            for (int i = 0; i < state.WeaponArray.Length; ++i)
            {
                state.WeaponArray[i].WeaponId = i;
                state.WeaponArray[i].WeaponAmmo = Random.Range(50, 100);
            }

            //NEW: by default we don't have any weapon up, so set index to -1
            state.WeaponActiveIndex = -1;
        }
        state.AddCallback("CubeColor", ColorChanged);
        state.AddCallback("WeaponActiveIndex", WeaponActiveIndexChanged);
        renderer = GetComponent<Renderer>();
    }

    void OnGUI()
    {
        if (entity.isOwner)
        {
            GUI.color = state.CubeColor;
            GUILayout.Label("@@@");
            GUI.color = Color.white;
        }
    }

    //Equivalent de l'update
    public override void SimulateOwner()
    {
        var speed = 4f;
        var movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) { movement.z += 1; }
        if (Input.GetKey(KeyCode.S)) { movement.z -= 1; }
        if (Input.GetKey(KeyCode.A)) { movement.x -= 1; }
        if (Input.GetKey(KeyCode.D)) { movement.x += 1; }

        // NEW: Input polling for weapon selection
        if (Input.GetKeyDown(KeyCode.Alpha1)) state.WeaponActiveIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) state.WeaponActiveIndex = 1;
        if (Input.GetKeyDown(KeyCode.Alpha3)) state.WeaponActiveIndex = 2;
        if (Input.GetKeyDown(KeyCode.Alpha0)) state.WeaponActiveIndex = -1;

        if (movement != Vector3.zero)
        {
            transform.position = transform.position + (movement.normalized * speed * BoltNetwork.FrameDeltaTime);  //FrameDeltaTime = Time.fixedDeltaTime 
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            var flash = FlashColorEvent.Create(entity);
            flash.FlashColor = Color.red;
            flash.Send();
        }

        
    }

    void Update()
    {
        if (resetColorTime < Time.time)
        {
            renderer.material.color = state.CubeColor;
        }
    }

    void ColorChanged()
    {
        GetComponent<Renderer>().material.color = state.CubeColor;
    }

    void WeaponActiveIndexChanged()
    {
        for (int i = 0; i < WeaponObjects.Length; ++i)
        {
            WeaponObjects[i].SetActive(false);
        }

        if (state.WeaponActiveIndex >= 0)
        {
            int objectId = state.WeaponArray[state.WeaponActiveIndex].WeaponId;
            WeaponObjects[objectId].SetActive(true);
        }
    }

    //Entity event prend 1 parametre, sert a faire du feedback, jouer une explosion, etc
    public override void OnEvent(FlashColorEvent evnt)
    {
        resetColorTime = Time.time + 0.2f;
        renderer.material.color = evnt.FlashColor;
    }
}
