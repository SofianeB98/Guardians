using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AxeTraining : MonoBehaviour
{
    [Header("Info Base")]
    [SerializeField] private Rigidbody rigid;
    [SerializeField] private Transform myHandParent;
    [SerializeField] private GuardianTraining myGuardian;

    [Header("Gestion Lancer De Hache")]
    [SerializeField] private float axeRadiusLaunchCheck = 1f;
    [SerializeField] private Transform pointOneAxeLaunch;
    [SerializeField] private Transform pointTwoAxeLaunch;

    private bool canLauchAxe = true;
    public bool CanLauchAxe
    {
        get { return canLauchAxe; }
    }

    private bool backToBucheronPos = false;
    public bool BackToBucheron
    {
        get { return backToBucheronPos; }
    }

    [Header("Gestion du Mouvement")]
    [SerializeField] private float axeReachTime = 0.5f;
    private float axeLaunchTimer = 0f;
    [SerializeField] private AnimationCurve axeLaunchForwardBehaviour;
    [SerializeField] private float currentAxeReachForwardDistance = 10f;
    //[SerializeField] private AnimationCurve axeLaunchSideBehaviour;
    //[SerializeField] private float currentAxeReachSideDistance = 3f;
    [SerializeField] private float axeBackSpeed = 10f;
    //[SerializeField] [Range(0.5f, 1.5f)] private float distYGround = 1f;
    [SerializeField] private LayerMask groundLayerMask;
    private Vector3 axeInitPos;
    private Quaternion axeInitRotate;
    [SerializeField] private LayerMask ignoreLayerMask;
    private Quaternion axeOrientation;
    [SerializeField] private float forcePush = 10f;

    [Header("FeedBack")]
    [SerializeField] private TrailRenderer myTrail;
    [SerializeField] private GameObject contactParticulePrefab;

    [Header("Audio")]
    [FMODUnity.EventRef]
    [SerializeField] private string collisionPlayerEvent = "";
    [SerializeField] private FMOD.Studio.EventInstance collisionPlayer;
    [FMODUnity.EventRef]
    [SerializeField] private string collisionDecorEvent = "";
    [SerializeField] private FMOD.Studio.EventInstance collisionDecor;
    

    private void Awake()
    {
        this.rigid = this.GetComponent<Rigidbody>();
        this.axeInitPos = this.transform.localPosition;
        this.axeInitRotate = this.transform.localRotation;
        if(this.transform.parent == null) BoltNetwork.Destroy(this.gameObject);

        collisionPlayer = FMODUnity.RuntimeManager.CreateInstance(collisionPlayerEvent);
        collisionDecor = FMODUnity.RuntimeManager.CreateInstance(collisionDecorEvent);
        
    }

    private void Update()
    {
        if (!canLauchAxe && !backToBucheronPos)
        {
            LaunchAxe();
        }
        else if (this.backToBucheronPos)
        {
            BackToBucheronPos();
        }
    }
    
    private void LaunchAxe()
    {
        this.axeLaunchTimer += Time.deltaTime / this.axeReachTime;

        if (this.axeLaunchTimer >= 1f)
        {
            this.backToBucheronPos = true;
            this.axeLaunchTimer = 0f;
        }
        else if(this.axeLaunchTimer < 1f)
        {
            var forwardVelocity =
                this.axeLaunchForwardBehaviour.Evaluate(this.axeLaunchTimer + Time.deltaTime / this.axeReachTime)
                                  - this.axeLaunchForwardBehaviour.Evaluate(this.axeLaunchTimer);

           // var sideVelocity =
           //     this.axeLaunchSideBehaviour.Evaluate(this.axeLaunchTimer + Time.deltaTime / this.axeReachTime)
            //                   - this.axeLaunchSideBehaviour.Evaluate(this.axeLaunchTimer);
            //
            //Vector3 sideVector = new Vector3(sideVelocity * this.currentAxeReachSideDistance / Time.deltaTime,0,0);
            
            Vector3 forwardVector = new Vector3(0,0,forwardVelocity * this.currentAxeReachForwardDistance / Time.deltaTime);

            Vector3 direction = axeOrientation * forwardVector;//(sideVector + forwardVector);
            
            this.rigid.velocity = direction;
            this.transform.position = rigid.position;
        }
    }

    private void BackToBucheronPos()
    {
        if (this.backToBucheronPos)
        {
            this.rigid.isKinematic = true;
            this.transform.position = Vector3.MoveTowards(this.transform.position, myGuardian.transform.position, Time.deltaTime * axeBackSpeed);

            if (Vector3.Distance(this.transform.position, myGuardian.transform.position) <= 1f)
            {
                this.isCanLaunchAxe(true, Quaternion.identity);
                this.backToBucheronPos = false;
                lastGuardian = null;
            }
        }
    }

    public void ActiveBackToBucheron()
    {
        if (!this.backToBucheronPos)
        {
            this.backToBucheronPos = true;
            this.axeLaunchTimer = 0f;
        }
    }

    private GuardianTraining lastGuardian = null;
    IEnumerator CheckObject()
    {
        while (!this.canLauchAxe)
        {
            yield return new WaitForEndOfFrame();
            bool check = true;
            bool objetFind = false;
            
            Collider[] col = Physics.OverlapCapsule(this.pointOneAxeLaunch.position, this.pointTwoAxeLaunch.position, axeRadiusLaunchCheck, ~ignoreLayerMask);
            Vector3 dir = myGuardian.transform.position - this.transform.position;
            if (col != null && check && !objetFind)
            {
                for (int i = 0; i < col.Length; i++)
                {
                    GuardianTraining g = col[i].GetComponent<GuardianTraining>();
                    if (g != null)
                    {
                        if (g != myGuardian && g != lastGuardian)//&& !g.IsStuned)
                        {
                            if (!g.IsStuned && !g.IsDie && !g.IsInvinsible)
                            {
                                lastGuardian = g;

                                if (!this.BackToBucheron)
                                {
                                    dir = -dir;
                                }

                                dir.y = 0;
                                g.SetStun(dir.normalized, forcePush);
                                objetFind = true;
                                /////Son
                                collisionPlayer.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
                                collisionPlayer.start();
                                /////Son

                                GameObject go = Instantiate(contactParticulePrefab, this.transform.position,
                                    Quaternion.LookRotation(-dir, Vector3.up));
                                Destroy(go, 1f);

                                check = false;
                            }

                        }
                    }
                    else
                    {
                        /////Son
                        collisionDecor.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
                        collisionDecor.start();
                        /////Son
                        objetFind = true;
                        check = false;
                    }
                }
            }

            if (objetFind)
            {
                ActiveBackToBucheron();
            }
        }
        yield break;
    }

    /*IEnumerator CheckDistanceGround()
    {
        yield return new WaitForSeconds(0.1f);
        while (!this.canLauchAxe)
        {
            bool check = true;
            yield return new WaitForEndOfFrame();
            RaycastHit hit;
            Ray ray = new Ray(this.transform.position, Vector3.down);

            if (Physics.Raycast(ray, out hit, 500f, this.groundLayerMask) && check)
            {
                if (hit.transform != null)
                {
                    float dist = Vector3.Distance(this.transform.position, hit.point);
                    if (dist > this.distYGround)
                    {
                        float newDist = this.transform.position.y - (dist - this.distYGround); 
                        this.transform.position = new Vector3(this.transform.position.x, newDist, this.transform.position.z);
                    }
                    else if (dist < this.distYGround)
                    {
                        float newDist = this.transform.position.y + (this.distYGround - dist);
                        this.transform.position = new Vector3(this.transform.position.x, newDist, this.transform.position.z);
                    }
                }

                check = false;

            }
        }
        yield break;
    }*/
    
    public void isCanLaunchAxe(bool canLaunch, Quaternion orientation)
    {
        this.axeOrientation = orientation;
        this.canLauchAxe = canLaunch;

        if (!this.canLauchAxe)
        {
            this.myTrail.enabled = true;

            this.rigid.isKinematic = false;

            this.transform.parent = null;

            this.transform.eulerAngles = new Vector3(0, 0, 0);

            this.transform.position = this.myHandParent.transform.position;

            StartCoroutine(this.CheckObject());
            //StartCoroutine(this.CheckDistanceGround());
        }
        else if (this.canLauchAxe)
        {
            this.myTrail.enabled = false;
            this.rigid.isKinematic = true;
            this.transform.parent = myHandParent.transform;
            this.transform.localPosition = Vector3.zero;
            myGuardian.SetLaunchAxe(false);
            //this.transform.localRotation = axeInitRotate;
        }
        
    }
}

