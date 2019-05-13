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
    [SerializeField] private float pillierLifeTime = 15.0f;
    [SerializeField] private float maxDistanceWithMyGuardian = 20.0f;
    private bool destroy = false;

    [Header("Rotate Laser")]
    [SerializeField] private Animator anim;
    //[SerializeField] private float maxRotation = 90.0f;
    [SerializeField] private float speedRotation = 5f;
    [SerializeField] private float animationScaleDuration = 1.0f;
    private float currentDuration = 0f;
    [SerializeField] [Range(1.0f, 5.0f)] private float animationSpeed = 1.0f;
    //[SerializeField] private float speedScalePillier = 4f;
    //[SerializeField] private float scaleMaxPillier = 4f;
    //private bool reverseRotate = false;
    //private float currentAngleRotate = 45f;
    private int currentDir = 1;

    [Header("Laser")]
    [SerializeField] private LayerMask checkLayer;
    [SerializeField] private int damage = 1;

    [SerializeField] private LayerMask groundLayerMask;
    private Vector3 plateformPosition = Vector3.zero;
    private Vector3 distPlateform = Vector3.zero;

    private void Start()
    {
        this.anim.SetFloat("SpeedScale", this.animationSpeed);
        this.anim.SetBool("IsScaling", true);
    }

    public void Init(Color myOwnerColor, int dir, GuardianTraining g)
    {
        this.currentDir = dir;
        myOwnerColor.a = 0.333f;
        this.myguardian = g;
        this.currentDuration = Time.time + (this.animationScaleDuration / this.animationSpeed);
    }

    private void Update()
    {
        CheckPlateformMouvante();
        if (this.plateformPosition != Vector3.zero) this.transform.position = this.plateformPosition - this.distPlateform;

        if (Time.time > this.currentDuration && this.anim.GetBool("IsScaling") == true)
        {
            this.anim.SetBool("IsScaling", false);
        }

        if (!this.laserGO.activeSelf && !this.anim.GetBool("IsScaling"))
        {
            this.laserGO.SetActive(true);
            if (doubleLaser)
            {
                this.laserDeuxGo.SetActive(true);
            }
            StartCoroutine(LaunchCheck());
        }
        else if (this.laserGO.activeSelf && !this.anim.GetBool("IsScaling"))
        {
            this.RotateLaser();
            if (this.pillierLifeTime > 0 && Vector3.Distance(this.transform.position, myguardian.transform.position) < this.maxDistanceWithMyGuardian)
            {
                this.pillierLifeTime -= Time.deltaTime;
            }
            else
            {
                this.DestroyPillier();
            }
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
        myguardian.RemovePillier(this);
        Destroy(this.gameObject);
    }

    private void RotateLaser()
    {
        this.transform.eulerAngles += Vector3.up * Time.deltaTime * this.speedRotation * this.currentDir;
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