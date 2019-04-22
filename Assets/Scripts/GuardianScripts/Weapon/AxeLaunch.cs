using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AxeLaunch : MonoBehaviour
{
    [Header("Info Base")]
    [SerializeField] private Rigidbody rigid;
    [SerializeField] private GameObject axeGraphics;
    [SerializeField] private Transform myHandParent;
    [SerializeField] private Animator axeAnim;
    [SerializeField] private Guardian myGuardian;

    [Header("Gestion Lancer De Hache")]
    [SerializeField] [Range(0,100)] private float axeLaunchCost = 5f;
    public float AxeLaunchCost
    {
        get { return axeLaunchCost; }
    }

    [SerializeField] private float axeRadiusLaunchCheck = 1f;
    [SerializeField] private float axeDistanceLaunchCheck = 1f;
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

    private float axeLaunchTimer = 0f;
    [SerializeField] private float axeReachTime = 0.5f;

    [SerializeField] private AnimationCurve axeLaunchForwardBehaviour;
    [SerializeField] private float speedForwardIncrementation = 2f;
    [SerializeField] private float axeReachForwardDistanceMAX = 20f;
    [SerializeField] private float axeReachForwardDistanceMIN = 5f;
    private float currentAxeReachForwardDistance = 10f;
    [SerializeField] private AnimationCurve axeLaunchSideBehaviour;
    [SerializeField] private float speedSideIncrementation = 2f;
    [SerializeField] private float axeReachSideDistanceMAX = 10f;
    private float currentAxeReachSideDistance = 3f;
    public float CurrentAxeReachSideDistance
    {
        get { return currentAxeReachSideDistance; }
    }
    [SerializeField] private float axeBackSpeed = 10f;
    [SerializeField] [Range(0.5f, 1.5f)] private float distYGround = 1f;
    [SerializeField] private LayerMask groundLayerMask;
    private int combuCut = 0;
    private Vector3 axeInitPos;
    private Quaternion axeInitRotate;
    
    private Quaternion bucheronRotation;
    
    private void Awake()
    {
        this.rigid = this.GetComponent<Rigidbody>();
        this.axeInitPos = this.transform.localPosition;
        this.axeInitRotate = this.transform.localRotation;
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

        if (this.axeLaunchTimer >= 0.99f)
        {
            this.backToBucheronPos = true;
            this.axeLaunchTimer = 0f;
        }
        else if(this.axeLaunchTimer < 0.99f)
        {
            var forwardVelocity =
                this.axeLaunchForwardBehaviour.Evaluate(this.axeLaunchTimer + Time.deltaTime / this.axeReachTime)
                                  - this.axeLaunchForwardBehaviour.Evaluate(this.axeLaunchTimer);

            var sideVelocity =
                this.axeLaunchSideBehaviour.Evaluate(this.axeLaunchTimer + Time.deltaTime / this.axeReachTime)
                               - this.axeLaunchSideBehaviour.Evaluate(this.axeLaunchTimer);
            
            Vector3 sideVector = new Vector3(sideVelocity * this.currentAxeReachSideDistance / Time.deltaTime,0,0);
            
            Vector3 forwardVector = new Vector3(0,0,forwardVelocity * this.currentAxeReachForwardDistance / Time.deltaTime);

            Vector3 direction = bucheronRotation * (sideVector + forwardVector);


            //this.rigid.MovePosition(this.rigid.position + direction);
            this.rigid.velocity = direction;
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

    IEnumerator CheckObject()
    {
        yield return new WaitForSeconds(0.1f);

        while (!this.canLauchAxe)
        {

            bool check = true;
            yield return new WaitForEndOfFrame();
            Collider[] col = Physics.OverlapCapsule(this.pointOneAxeLaunch.position, this.pointTwoAxeLaunch.position, axeRadiusLaunchCheck);
            if (col != null && check)
            {
                for (int i = 0; i < col.Length; i++)
                {
                    /*Pillier colWood = col[i].GetComponent<Pillier>();
                    if (colWood != null && currentWood != colWood)
                    {
                        currentWood = colWood;
                        this.myGuardian.MonsterPooling.AddWood(currentWood.transform.position);
                        colWood.TakeDamage();
                        this.combuCut = colWood.contaminated == false ? this.combuCut + 1 : this.combuCut + 2;
                    }*/
                    
                }

                check = false;
            }
        }
        yield break;
    }

    IEnumerator CheckDistanceGround()
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
    }

    public void isCanLaunchAxe(bool canLaunch, Quaternion bucheronRotation)
    {
        this.bucheronRotation = new Quaternion(0, bucheronRotation.y, 0, bucheronRotation.w);

        if (!canLaunch)
        {
            this.rigid.isKinematic = false;
            

            this.transform.parent = null;

            this.transform.eulerAngles = new Vector3(0,0,0);

            this.transform.position = this.myGuardian.transform.position + Vector3.up;

            this.axeAnim.enabled = true;
            
            StartCoroutine(this.CheckObject());
            StartCoroutine(this.CheckDistanceGround());
        }
        else if(canLaunch)
        {
            this.rigid.isKinematic = true;
            this.axeAnim.enabled = false;
            this.transform.parent = myHandParent.transform;
            this.transform.localPosition = axeInitPos;
            this.transform.localRotation = axeInitRotate;
            this.axeGraphics.transform.localPosition = Vector3.zero;
            this.axeGraphics.transform.localRotation = Quaternion.Euler(0,0,0);
            
            
        }

        //myGuardian.SetLaunchAxe(!canLaunch);
        canLauchAxe = canLaunch;
    }
}

