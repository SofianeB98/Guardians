using System;
using System.Collections;
using System.Collections.Generic;
using Bolt;
using UnityEngine;

public class PillierTraining : MonoBehaviour
{
    [SerializeField] private float health = 1.0f;
    private GuardianTraining myguardian;

    [Header("Base")]
    [SerializeField] private GameObject pillierGO;
    [SerializeField] private GameObject laserGO;
    [SerializeField] private GameObject laserDeuxGo;
    [SerializeField] private Transform seedDrop;
    [SerializeField] private Renderer rdToColor;
    [SerializeField] private bool doubleLaser = true;
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

    [SerializeField] private LayerMask groundLayerMask;
    private Vector3 plateformPosition = Vector3.zero;
    private Vector3 distPlateform = Vector3.zero;

    public void Init(Color myOwnerColor, int dir, GuardianTraining g)
    {
        this.currentDir = dir;
        myOwnerColor.a = 0.75f;
        this.myguardian = g;
    }

    private void Update()
    {
        CheckPlateformMouvante();
        if (this.plateformPosition != Vector3.zero) this.transform.position = this.plateformPosition - this.distPlateform;

        if (this.pillierGO.transform.localScale.y < this.scaleMaxPillier)
        {
            this.pillierGO.transform.localScale += BoltNetwork.FrameDeltaTime * Vector3.up * this.speedScalePillier;
        }
        else if (!this.laserGO.activeSelf)
        {
            this.laserGO.SetActive(true);
            if (doubleLaser)
            {
                this.laserDeuxGo.SetActive(true);
            }
            StartCoroutine(LaunchCheck());
        }
        else
        {
            this.RotateLaser();
            
        }
    }

    private void CheckPlateformMouvante()
    {
        RaycastHit pmHit;
        if (Physics.SphereCast(this.transform.position + Vector3.up, 1f, Vector3.down, out pmHit, 1,
            groundLayerMask))
        {
            if (pmHit.transform.tag.Contains("PMouvante"))
            {
                this.plateformPosition = pmHit.transform.position;
                if (this.distPlateform == Vector3.zero) this.distPlateform = this.plateformPosition - this.transform.position;
            }
        }
    }

    #region PillierFunction

    public void DestroyPillier()
    {
        //SeedTraining s = BoltNetwork.Instantiate(BoltPrefabs.Seed, this.seedDrop.position, Quaternion.identity).GetComponent<Seed>();
        //s.Init(0, null, Quaternion.identity, false, myOwner, state.MyColor, this.currentDir);
        Destroy(this.gameObject);
        myguardian.RemovePillier(this);
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
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            this.CheckPlayer();
            if (doubleLaser)
            {
                CheckPlayerDouble();
            }
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

    private void CheckPlayerDouble()
    {
        Collider[] col = Physics.OverlapBox(this.laserDeuxGo.transform.position, this.laserDeuxGo.transform.localScale / 2,
            this.laserDeuxGo.transform.rotation, this.checkLayer);
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
                        return;
                    }

                    return;
                }
            }
        }
        return;
    }

    #endregion
}