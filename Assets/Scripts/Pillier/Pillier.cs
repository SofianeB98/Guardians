using System.Collections;
using System.Collections.Generic;
using Bolt;
using UnityEngine;

public class Pillier : Bolt.EntityEventListener<IPillierState>
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

    public override void Attached()
    {
        state.SetTransforms(state.Transform, this.transform);
        if (entity.IsOwner)
        {
            state.Active = laserGO.activeSelf;
            state.Scale = pillierGO.transform.localScale;
            
            entity.TakeControl();
        }
        state.AddCallback("Active", ActiveLaser);
        state.AddCallback("Scale", ScalePillier);
        state.AddCallback("MyColor", ColorChanged);
    }

    public void Init(BoltEntity ent, Color myOwnerColor, int dir)
    {
        this.currentDir = dir;
        this.myOwner = ent;
        if (entity.IsOwner)
        {
            myOwnerColor.a = 0.75f;
            state.MyOwner = ent;
            state.MyColor = myOwnerColor;
            
        }
    }

    public override void SimulateOwner()
    {
        if (this.pillierGO.transform.localScale.y < this.scaleMaxPillier)
        {
            state.Scale += BoltNetwork.FrameDeltaTime * Vector3.up * this.speedScalePillier;
        }
        else if (!this.laserGO.activeSelf)
        {
            state.Active = true;
            StartCoroutine(LaunchCheck());
        }
        else
        {
            this.RotateLaser();
            
        }
    }
    
    #region PillierFunction

    private void ScalePillier()
    {
        this.pillierGO.transform.localScale = state.Scale;
    }

    private void ActiveLaser()
    {
        this.laserGO.SetActive(state.Active);
    }

    public void DestroyPillier()
    {
        Seed s = BoltNetwork.Instantiate(BoltPrefabs.Seed, this.seedDrop.position, Quaternion.identity).GetComponent<Seed>();
        s.Init(0, null, Quaternion.identity, false, myOwner, state.MyColor, this.currentDir);
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
    }

    private void RotateLaser()
    {
        //if (!reverseRotate)
        {
            //if (currentAngleRotate < 90)
            {
                this.transform.eulerAngles += Vector3.up * BoltNetwork.FrameDeltaTime * this.speedRotation * this.currentDir;
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
                            myOwner.GetComponent<Guardian>().UpdateScore(false);
                        }
                        else
                        {
                            myOwner.GetComponent<Guardian>().UpdateScore(true);
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

    void ColorChanged()
    {
        rdToColor.material.color = state.MyColor;
    }

}