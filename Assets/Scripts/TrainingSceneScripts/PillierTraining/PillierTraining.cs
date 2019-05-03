﻿using System.Collections;
using System.Collections.Generic;
using Bolt;
using UnityEngine;

public class PillierTraining : MonoBehaviour
{
    private BoltEntity myOwner;
    [SerializeField] private float health = 1.0f;

    [Header("Base")]
    [SerializeField] private GameObject pillierGO;
    [SerializeField] private GameObject laserGO;
    [SerializeField] private Transform seedDrop;
    [SerializeField] private Renderer rdToColor;
    private bool destroy = false;

    [Header("Rotate Laser")]
    [SerializeField] private float maxRotation = 90.0f;
    [SerializeField] private float speedRotation = 5f;
    [SerializeField] private float speedScalePillier = 4f;
    [SerializeField] private float scaleMaxPillier = 4f;
    private bool reverseRotate = false;
    private float currentAngleRotate = 45f;
    private int currentDir = 1;

    [Header("Laser")]
    [SerializeField] private LayerMask checkLayer;
    [SerializeField] private int damage = 1;

    

    public void Init(Color myOwnerColor, int dir)
    {
        this.currentDir = dir;
        myOwnerColor.a = 0.75f;
        
    }

    private void Update()
    {
        if (this.pillierGO.transform.localScale.y < this.scaleMaxPillier)
        {
            this.pillierGO.transform.localScale += BoltNetwork.FrameDeltaTime * Vector3.up * this.speedScalePillier;
        }
        else if (!this.laserGO.activeSelf)
        {
            this.laserGO.SetActive(true);
            StartCoroutine(LaunchCheck());
        }
        else
        {
            this.RotateLaser();
            
        }
    }
    
    #region PillierFunction
    
    public void DestroyPillier()
    {
        //SeedTraining s = BoltNetwork.Instantiate(BoltPrefabs.Seed, this.seedDrop.position, Quaternion.identity).GetComponent<Seed>();
        //s.Init(0, null, Quaternion.identity, false, myOwner, state.MyColor, this.currentDir);
        //Destroy(this.gameObject);
    }

    private void RotateLaser()
    {
        //if (!reverseRotate)
        {
            //if (currentAngleRotate < 90)
            {
                this.transform.eulerAngles += Vector3.up * Time.deltaTime * this.speedRotation * this.currentDir;
                //this.currentAngleRotate += BoltNetwork.FrameDeltaTime * this.speedRotation;
            }
            //else
            {
                //this.reverseRotate = true;
            }
        }
        //else
        {
            //if (currentAngleRotate > 0)
            {
            //    this.transform.eulerAngles -= Vector3.up * BoltNetwork.FrameDeltaTime * this.speedRotation;
                //this.currentAngleRotate -= BoltNetwork.FrameDeltaTime * this.speedRotation;
            }
           // else
            {
             //   this.reverseRotate = false;
            }
        }

    }

    private void RotatePillier(float rotationAngle)
    {
        var newRotate = this.transform.eulerAngles.y + rotationAngle;
        this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, newRotate, this.transform.eulerAngles.z);
    }

    #endregion

    #region PlayerInteraction

    IEnumerator LaunchCheck()
    {
        yield return new WaitForEndOfFrame();
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            this.CheckPlayer();
        }
        yield break;
    }

    private void CheckPlayer()
    {
        Collider[] col = Physics.OverlapBox(this.laserGO.transform.position, this.laserGO.transform.localScale / 2,
            this.laserGO.transform.rotation, this.checkLayer);
        if (col.Length > 0 && col != null)
        {
            for (int i = 0; i < col.Length; i++)
            {
                GuardianTraining g = col[i].GetComponent<GuardianTraining>();
                if (g != null)
                {
                    if (!g.IsInvinsible && !g.IsDie)
                    {
                        g.TakeDamage(this.damage);
                        Debug.Log("Gardian toucher");
                    }

                    return;
                }
            }
        }
        return;
    }

    #endregion
}