using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bolt;
using UnityEngine;

public class SeedTraining : MonoBehaviour
{
    private BoltEntity myOwner;
    [SerializeField] private Rigidbody rigid;
    [SerializeField] private Quaternion pillierRotate;
    [SerializeField] private GuardianTraining myGuardian;
    [SerializeField] private int myTeam;
    [SerializeField] private string groundTag = "Ground";
    private bool isLaunchPlayer = false;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private TrailRenderer trailRenderer;
    private int currentDir = 1;
    [SerializeField] private PillierTraining pillier;

    //Son
    [FMODUnity.EventRef]
    public string collisionObstacleEvent;
    public FMOD.Studio.EventInstance collisionObstacle;
    
    private void Awake()
    {
        collisionObstacle = FMODUnity.RuntimeManager.CreateInstance(collisionObstacleEvent);
    }

    public void Init(int team, GuardianTraining guardian, Quaternion rotation, bool launchPlayer, int dir)
    {
        this.myTeam = team;
        this.myGuardian = guardian;
        
        this.currentDir = dir;
        this.pillierRotate = rotation;
        this.isLaunchPlayer = launchPlayer;
        this.rigid.isKinematic = false;
    }
    

    public void InitVelocity(float force, Vector3 dir)
    {
        this.rigid.isKinematic = false;
        this.rigid.AddForce(dir * force, ForceMode.Impulse);
    }

    private void Update()
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
            PillierTraining p = Instantiate(pillier, this.transform.position - new Vector3(0, 0.4f, 0), this.pillierRotate);
            p.Init(Color.white,this.currentDir);
            this.myGuardian.AddPillierToMyList(p);
            Destroy(this.gameObject);
        }
    }

    public void DestroyOnPickUp()
    {
        Destroy(this.gameObject);
    }
    
}
