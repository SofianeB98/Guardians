using System.Collections;
using System.Collections.Generic;
using Bolt;
using UnityEngine;

public class Pillier : Bolt.EntityEventListener<IPillierState>
{
    private BoltEntity myOwner;
    [SerializeField] private float health = 1.0f;

    private RotatePlateformMovement rpm = null;

    [Header("Base")]
    [SerializeField] private GameObject pillierGO;
    [SerializeField] private GameObject laserGO;
    [SerializeField] private GameObject laserDeuxGo;
    [SerializeField] private Transform seedDrop;
    [SerializeField] private Renderer laserRD;
    [SerializeField] private Renderer laserDeuxRD;
    [SerializeField] private bool doubleLaser = true;
    [SerializeField] private float pillierLifeTime = 15.0f;
    [SerializeField] private float maxDistanceWithMyGuardian = 20.0f;
    private bool destroy = false;

    [Header("Rotate Laser")]
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
    private float anglePositionRecal = 1000;
    private Vector3 distPlateform = Vector3.zero;

    public override void Attached()
    {
        state.SetTransforms(state.Transform, this.transform);
        state.SetAnimator(GetComponentInChildren<Animator>());
        
        if (entity.IsOwner)
        {
            state.Active = laserGO.activeSelf;
            state.Scale = pillierGO.transform.localScale;
            state.IsScaling = true;
            state.SpeedScale = animationSpeed;
            entity.TakeControl();
        }
        state.AddCallback("Active", ActiveLaser);
        //state.AddCallback("Scale", ScalePillier);
        state.AddCallback("MyColor", ColorChanged);
    }

    public void Init(BoltEntity ent, Color myOwnerColor, int dir, Vector3 point, Vector3 normal)
    {
        this.currentDir = dir;
        this.myOwner = ent;
        this.currentDuration = Time.time + (this.animationScaleDuration/this.animationSpeed);
        if (entity.IsOwner)
        {
            myOwnerColor.a = 0.75f;
            state.MyOwner = ent;
            state.MyColor = myOwnerColor;
        }

        this.plateformPosition = point;

        if (normal != Vector3.zero)
        {
            this.transform.rotation = Quaternion.FromToRotation(this.transform.up, normal);
        }

        //CheckPlateformMouvante();
        //StartCoroutine(PositionActualliser());
    }

    public override void SimulateOwner()
    {
        if (rpm != null)
        {
            if (anglePositionRecal > 999) this.anglePositionRecal = rpm.AngleToRotate(this.plateformPosition);
            //this.plateformPosition = rpm.FinalPos(anglePositionRecal, this.plateformPosition);
            this.transform.position = rpm.FinalPos(anglePositionRecal, this.transform.position);
        }
        else
        {
            CheckPlateformMouvante();
        }

        if (Time.time > this.currentDuration && state.IsScaling)
        {
            state.IsScaling = false;
            //state.Scale += BoltNetwork.FrameDeltaTime * Vector3.up * this.speedScalePillier;
        }

        if (!this.laserGO.activeSelf && !state.IsScaling)
        {
            state.Active = true;
            StartCoroutine(LaunchCheck());
        }
        else if(this.laserGO.activeSelf && !state.IsScaling)
        {
            this.RotateLaser();
            if (this.pillierLifeTime > 0 && Vector3.Distance(this.transform.position, myOwner.transform.position) < this.maxDistanceWithMyGuardian)
            {
                this.pillierLifeTime -= BoltNetwork.FrameDeltaTime;
            }
            else
            {
                DestroyPillier();
            }
        }
    }
    
    #region PillierFunction

    //private void ScalePillier()
    //{
        //this.pillierGO.transform.localScale = state.Scale;
    //}

    private void ActiveLaser()
    {
        this.laserGO.SetActive(state.Active);
        if (this.doubleLaser)
        {
            this.laserDeuxGo.SetActive(state.Active);
        }
    }

    public void DestroyPillier()
    {
        //Seed s = BoltNetwork.Instantiate(BoltPrefabs.Seed, this.seedDrop.position, Quaternion.identity).GetComponent<Seed>();
        //s.Init(0, null, Quaternion.identity, false, myOwner, state.MyColor, this.currentDir);
        if (entity.IsOwner)
        {
            BoltNetwork.Destroy(this.gameObject);
        }
        else
        {
            var evnt = DestroyEvent.Create(state.MyOwner, EntityTargets.Everyone);
            evnt.EntityDestroy = entity;
            evnt.Send();
        }
        this.myOwner.GetComponent<Guardian>().RemovePillier(this);
    }

    private void RotateLaser()
    {
        //this.transform.eulerAngles = this.transform.rotation * Vector3.up * BoltNetwork.FrameDeltaTime * this.speedRotation * this.currentDir;
        this.transform.RotateAround(this.transform.position, this.transform.up, BoltNetwork.FrameDeltaTime * this.speedRotation * this.currentDir);
    }

    private void CheckPlateformMouvante()
    {
        Collider[] col = Physics.OverlapSphere(this.transform.position, 1f, groundLayerMask);
        foreach (var truc in col)
        {
            Debug.Log(truc.name);
            if (truc.transform.tag.Contains("PMouvante"))
            {
                rpm = truc.gameObject.GetComponentInParent<RotatePlateformMovement>();
                Debug.Log("Find !");
            }

            if (truc == col[col.Length - 1] && rpm == null)
            {
                this.plateformPosition = Vector3.zero;
            }
        }
    }

    #endregion

    #region PlayerInteraction

    IEnumerator LaunchCheck()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (this.laserGO.activeSelf)
            {
                this.CheckPlayer();
                if (this.doubleLaser)
                {
                    CheckPlayerDouble();
                }
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
                Guardian g = col[i].GetComponent<Guardian>();
                if (g != null)
                {
                    if (!g.IsInvinsible && !g.IsDie)
                    {
                        string s = myOwner.GetComponent<Guardian>().guardianName + " kills " + g.guardianName;

                        g.TakeDamage(this.damage);
                        Debug.Log("Gardian toucher");

                        if (g != myOwner.GetComponent<Guardian>())
                        {
                            myOwner.GetComponent<Guardian>().UpdateScore(false, "Enemy burned", true);
                        }
                        else
                        {
                            //myOwner.GetComponent<Guardian>().UpdateScore(true);
                            s = myOwner.GetComponent<Guardian>().guardianName + " kills himself !";
                        }

                        
                        var evnt = KillFeedEvent.Create(GameSystem.GSystem.entity);
                        evnt.Message = s;
                        evnt.RemoveFeed = false;
                        evnt.Send();

                        return;
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
                Guardian g = col[i].GetComponent<Guardian>();
                if (g != null)
                {
                    if (!g.IsInvinsible && !g.IsDie)
                    {
                        string s = myOwner.GetComponent<Guardian>().guardianName + " kills " + g.guardianName;

                        g.TakeDamage(this.damage);
                        Debug.Log("Gardian toucher");
                        if (g != myOwner.GetComponent<Guardian>())
                        {
                            myOwner.GetComponent<Guardian>().UpdateScore(false, "Enemy is fired", true);
                        }
                        else
                        {
                            //myOwner.GetComponent<Guardian>().UpdateScore(true);
                            s = myOwner.GetComponent<Guardian>().guardianName + " kills himself !";
                        }


                        var evnt = KillFeedEvent.Create(GameSystem.GSystem.entity);
                        evnt.Message = s;
                        evnt.RemoveFeed = false;
                        evnt.Send();
                    }

                    return;
                }
            }
        }
        return;
    }

    #endregion

    IEnumerator PositionActualliser()
    {
        while (this.plateformPosition != Vector3.zero)
        {
            yield return new WaitForEndOfFrame();
            if (rpm != null)
            {
                if (anglePositionRecal > 999) this.anglePositionRecal = rpm.AngleToRotate(this.plateformPosition);
                //this.plateformPosition = rpm.FinalPos(anglePositionRecal, this.plateformPosition);
                this.transform.position = rpm.FinalPos(anglePositionRecal, this.transform.position);
            }
            else
            {
                CheckPlateformMouvante();
            }
        }
        yield break;
    }

    void ColorChanged()
    {
        laserRD.material.color = state.MyColor;
        if (doubleLaser)
        {
            laserDeuxRD.material.color = state.MyColor;
        }
    }

}