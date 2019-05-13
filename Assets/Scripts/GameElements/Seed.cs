using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bolt;
using UnityEngine;

public class Seed : Bolt.EntityEventListener<ISeedState>
{
    private BoltEntity myOwner;
    [SerializeField] private Rigidbody rigid;
    [SerializeField] private Quaternion pillierRotate;
    [SerializeField] private Guardian myGuardian;
    [SerializeField] private int myTeam;
    [SerializeField] private string groundTag = "Ground";
    private bool isLaunchPlayer = false;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private TrailRenderer trailRenderer;
    private int currentDir = 1;

    //Son
    [FMODUnity.EventRef]
    public string collisionObstacleEvent;
    public FMOD.Studio.EventInstance collisionObstacle;
    
    public override void Attached()
    {
        collisionObstacle = FMODUnity.RuntimeManager.CreateInstance(collisionObstacleEvent);

        state.SetTransforms(state.Transform, this.transform);
        if (entity.IsOwner)
        {
            entity.TakeControl();
        }
        state.AddCallback("MyColor", ColorChanged);
    }

    public void Init(int team, Guardian guardian, Quaternion rotation, bool launchPlayer, BoltEntity ent, Color ownerColor, int dir)
    {
        this.myTeam = team;
        this.myGuardian = guardian;
        this.myOwner = ent;
        if (entity.IsOwner)
        {
            state.MyOwner = ent;
            state.MyColor = ownerColor;
        }

        this.currentDir = dir;
        this.pillierRotate = rotation;
        this.isLaunchPlayer = launchPlayer;
        this.rigid.isKinematic = false;
    }

    void ColorChanged()
    {
        GetComponent<Renderer>().material.color = state.MyColor;
        trailRenderer.endColor = state.MyColor;
        trailRenderer.startColor = state.MyColor;
    }

    public void InitVelocity(float force, Vector3 dir)
    {
        this.rigid.isKinematic = false;
        this.rigid.AddForce(dir * force, ForceMode.Impulse);
    }

    public override void SimulateOwner()
    {
        this.transform.position = this.rigid.position;
        CheckGround();
    }

    private void OnCollisionEnter(Collision col)
    {
        /////Son
        collisionObstacle.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
        collisionObstacle.start();
        /////Son

        if (!this.isLaunchPlayer)
        {
            if (col.transform.tag.Contains(this.groundTag))
            {
                this.rigid.isKinematic = true;
            }
        }
    }

    private void CheckGround()
    {
        bool raycast = Physics.Raycast(transform.position, Vector3.down, 0.5f, groundLayerMask);
        if (raycast)
        {
            Pillier p = BoltNetwork.Instantiate(BoltPrefabs.PillieCube, this.transform.position - new Vector3(0, 0.4f, 0), this.pillierRotate).GetComponent<Pillier>();
            p.Init(state.MyOwner, state.MyColor, this.currentDir);
            this.myGuardian.AddPillierToMyList(p);
            BoltNetwork.Destroy(this.gameObject);
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
