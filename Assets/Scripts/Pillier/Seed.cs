using System.Collections;
using System.Collections.Generic;
using Bolt;
using UnityEngine;

public class Seed : Bolt.EntityEventListener<ISeedState>
{
    private BoltEntity myOwner;

    [SerializeField] private Pillier prefabPillier;
    [SerializeField] private Rigidbody rigid;
    [SerializeField] private Quaternion pillierRotate;
    [SerializeField] private Guardian myGuardian;
    [SerializeField] private int myTeam;
    [SerializeField] private string groundTag = "Ground";
    private bool isLaunchPlayer = false;
    
    public override void Attached()
    {
        state.SetTransforms(state.Transform, this.transform);
        if (entity.IsOwner)
        {
            entity.TakeControl();
        }
    }

    public void Init(int team, Guardian guardian, Quaternion rotation, bool launchPlayer, BoltEntity ent)
    {
        this.myTeam = team;
        this.myGuardian = guardian;
        this.myOwner = ent;
        if (entity.IsOwner)
        {
            state.MyOwner = ent;
        }

        this.pillierRotate = rotation;
        this.isLaunchPlayer = launchPlayer;
        this.rigid.isKinematic = false;
    }

    public void InitVelocity(float force, Vector3 dir)
    {
        this.rigid.isKinematic = false;
        this.rigid.AddForce(dir * force, ForceMode.Impulse);
    }

    public override void SimulateOwner()
    {
        this.transform.position = this.rigid.position;
    }

    private void OnCollisionEnter(Collision col)
    {
        if (this.isLaunchPlayer)
        {
            if (col.transform.tag.Contains(this.groundTag))
            {
                Pillier p = BoltNetwork.Instantiate(BoltPrefabs.Pillier, this.transform.position, this.pillierRotate).GetComponent<Pillier>();
                p.Init(state.MyOwner);
                BoltNetwork.Destroy(this.gameObject);
            }
        }
        else
        {
            if (col.transform.tag.Contains(this.groundTag))
            {
                this.rigid.isKinematic = true;
            }
        }
    }

    public void DestroyOnPickUp()
    {
        if (entity.IsAttached)
        {
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
        
    }
    
}
