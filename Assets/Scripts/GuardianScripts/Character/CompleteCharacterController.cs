using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CompleteCharacterController : Bolt.EntityEventListener<IGuardianState>
{

	[SerializeField] private CharacterController characterController;
    private Vector3 plateformeMouvanteDir = Vector3.zero;
	private Vector3 direction = Vector3.zero;
	private Vector3 orientation = Vector3.forward;
	private Vector3 gravity = Vector3.zero;
	private Vector3 finalDirection = Vector3.zero;
	[SerializeField] private Transform cameraReferential;
	[SerializeField] [Range(1,15)] private float speed = 10;
    [SerializeField] private LayerMask groundLayerMask;
	private Vector3 groundPosition;
	private bool groundDetected;
	[SerializeField] private bool grounded;
    public bool Grounded
    {
        get { return grounded; }
    }
	private RaycastHit lastGroundDetectedInfos;

    [Header("Gravity")]
	[SerializeField] private float gravityForce = 9.81f;
	[SerializeField] private float gravityModifier = 1;
	[SerializeField] private float gravityMaxSpeed = 50;

    [Header("NE PAS TOUCHER")]
    [SerializeField] private float sphereGroundDetectionRadius = 0.4f;
	[SerializeField] private float groundTolerance = 0.05f;
	[SerializeField] private float characterControllerRadiusCompensator = 0.1f;

    [Header("Jump Section")]
    [SerializeField] private float speedPerteSpeed = 1f;
	[SerializeField] private float jumpHeight = 8;
	[SerializeField] private float jumpTimeToReachMax = 0.5f;
	[SerializeField] private AnimationCurve jumpBehaviour;
	private float jumpTimer;
    [SerializeField] private float doubleJumpHeight = 8;
    [SerializeField] private float doubleJumpTimeToReachMax = 0.5f;
    [SerializeField] private AnimationCurve doubleJumpBehaviour;
    private float doubleJumpTimer;
    public bool jumping { get; private set; }
    public bool doubleJumping { get; private set; }

    [Header("Audio")]
    [FMODUnity.EventRef]
    public string sautAudioEvent;
    public FMOD.Studio.EventInstance sautAudio;
    [FMODUnity.EventRef]
    public string colBalleMeEvent;
    public FMOD.Studio.EventInstance colBalleMe;
    [FMODUnity.EventRef]
    [SerializeField] private string launchSeedAudioEvent = "";
    [SerializeField] private FMOD.Studio.EventInstance launchSeedAudio;

    // Use this for initialization
    void Awake () {
		if (this.characterController == null) {
			this.characterController = this.GetComponent<CharacterController>();
		}

	    this.jumping = false;
	    this.doubleJumping = true;

        sautAudio = FMODUnity.RuntimeManager.CreateInstance(sautAudioEvent);
        colBalleMe = FMODUnity.RuntimeManager.CreateInstance(colBalleMeEvent);

        launchSeedAudio = FMODUnity.RuntimeManager.CreateInstance(launchSeedAudioEvent);
    }

    public override void Attached()
    {
        state.SetTransforms(state.Transform, transform);
        if (entity.IsOwner)
        {
            this.cameraReferential.parent = null;
        }
        else
        {
            this.cameraReferential.gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    public void CustomUpdate () {
        if (cameraReferential != null)
        {
            this.DetectGround();
            this.UpdateGravity();
            this.finalDirection = this.direction + this.gravity + this.plateformeMouvanteDir;
            this.characterController.Move(this.finalDirection * Time.deltaTime);
            this.transform.rotation = Quaternion.AngleAxis(this.cameraReferential.eulerAngles.y, Vector3.up);//Quaternion.LookRotation(this.orientation, Vector3.up);
            this.GroundPositionCorrection();
        }
		
	}

	public void UpdateDirection(Vector3 dir) {
	    if (cameraReferential != null)
	    {
	        var cameraAngle = this.cameraReferential.rotation;
	        this.cameraReferential.eulerAngles = new Vector3(0, this.cameraReferential.eulerAngles.y, 0);
	        dir = this.cameraReferential.rotation * dir;
	        this.cameraReferential.rotation = cameraAngle;
	        this.direction = dir * this.speed;
	    }
	}

    public void UpdateDirWhenImJumping()
    {
        this.direction = Vector3.Lerp(this.direction, Vector3.zero, Time.deltaTime * this.speedPerteSpeed);
        this.plateformeMouvanteDir = Vector3.zero;
    }

	public void UpdateRotation(Vector3 rot) {
	    if (cameraReferential != null)
	    {
	        var cameraAngle = this.cameraReferential.rotation;
	        this.cameraReferential.eulerAngles = new Vector3(0, this.cameraReferential.eulerAngles.y, 0);
	        rot = this.cameraReferential.rotation * rot;
	        this.cameraReferential.rotation = cameraAngle;
	        this.orientation = rot;
	    }
	}

	private void DetectGround() {
		this.groundDetected = Physics.SphereCast(new Vector3(this.transform.position.x, this.transform.position.y - this.characterController.height/2 + this.sphereGroundDetectionRadius - this.characterControllerRadiusCompensator, this.transform.position.z),
			this.sphereGroundDetectionRadius,
			Vector3.down,
			out this.lastGroundDetectedInfos,
			4,
		    groundLayerMask);


	    RaycastHit pmHit;
	    if (Physics.SphereCast(this.transform.position, 1f, Vector3.down, out pmHit, 0.5f,
	        groundLayerMask))
	    {
	        if (pmHit.transform.tag.Contains("PMouvante") && !this.jumping)
	        {
	            RotatePlateformMovement rpm = pmHit.transform.GetComponentInParent<RotatePlateformMovement>();
                PlateformMovement pm = pmHit.transform.GetComponent<PlateformMovement>();

	            if (rpm != null)
	            {
	                this.plateformeMouvanteDir = rpm.VectorDirecteurPlateforme(this.transform);
                }
                else if (pm != null)
	            {
	                this.plateformeMouvanteDir = pm.VectorDirecteurPlateforme();
                }
                
            }
	        else
	        {
	            this.plateformeMouvanteDir = Vector3.zero;
	        }
	    }

	}

	private void UpdateGravity() {
		if (!this.grounded && !this.jumping) {
			this.gravity = new Vector3(0,this.gravity.y - this.gravityForce * this.gravityModifier * Time.deltaTime,0);

			if (Mathf.Abs(this.gravity.y) > this.gravityMaxSpeed) {
				this.gravity = new Vector3(0,-this.gravityMaxSpeed,0);
			}
		}
		else if(this.jumping) {
			this.jumpTimer += Time.deltaTime / this.jumpTimeToReachMax;
			if (this.jumpTimer >= 1.0f) {
				this.jumping = false;
            }
			else {
				var velocity = this.jumpBehaviour.Evaluate(this.jumpTimer + Time.deltaTime / this.jumpTimeToReachMax) - this.jumpBehaviour.Evaluate(this.jumpTimer);
				this.gravity = new Vector3(0,velocity * this.jumpHeight / Time.deltaTime,0);
			}
		}
        else if (this.doubleJumping && this.jumping)
		{
		    this.doubleJumpTimer += Time.deltaTime / this.doubleJumpTimeToReachMax;
		    if (this.doubleJumpTimer >= 1.0f)
		    {
		        this.jumping = false;
		    }
		    else
		    {
		        var velocity = this.doubleJumpBehaviour.Evaluate(this.doubleJumpTimer + Time.deltaTime / this.doubleJumpTimeToReachMax) - this.doubleJumpBehaviour.Evaluate(this.doubleJumpTimer);
		        this.gravity = new Vector3(0, velocity * this.doubleJumpHeight / Time.deltaTime, 0);
		    }
        }
		else {
			this.gravity = new Vector3(0,0,0);
		    this.doubleJumping = false;
        }
	}

	private void GroundPositionCorrection() {

		if (this.transform.position.y - this.characterControllerRadiusCompensator - this.characterController.height/2 < this.lastGroundDetectedInfos.point.y - this.groundTolerance) {
			this.characterController.Move(new Vector3(0, Vector3.Distance(this.lastGroundDetectedInfos.point,new Vector3(this.transform.position.x,this.transform.position.y - this.characterControllerRadiusCompensator - this.characterController.height/2, this.transform.position.z)), 0));
			this.grounded = true;
		}
		else if(Mathf.Abs(this.transform.position.y - this.characterControllerRadiusCompensator - this.characterController.height/2 - this.lastGroundDetectedInfos.point.y) > this.groundTolerance){
			this.grounded = false;
		}
		else {
			this.grounded = true;
		}
	}

	public void UpdateJump() {
		if (this.grounded) {

		    this.doubleJumping = false;
            this.jumping = true;
		    
			this.jumpTimer = 0.0f;

		    this.plateformeMouvanteDir = Vector3.zero;

            var evnt = AudioStartEvent.Create(entity);
		    evnt.Position = transform.position;
		    evnt.AudioID = 0;
            evnt.Send();
		}
	}

    public void UpdateDoubleJump()
    {
        if (!this.grounded && !this.doubleJumping)
        {
            this.jumping = true;
            this.doubleJumping = true;
            this.jumpTimer = 0.0f;
            this.doubleJumpTimer = 0.0f;

            this.plateformeMouvanteDir = Vector3.zero;

            var evnt = AudioStartEvent.Create(entity);
            evnt.Position = transform.position;
            evnt.AudioID = 0;
            evnt.Send();
        }
    }

    public override void OnEvent(AudioStartEvent evnt)
    {
        switch (evnt.AudioID)
        {
            case 0:
                sautAudio.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(evnt.Position));
                sautAudio.start();
                break;

            case 1:
                /////Son
                launchSeedAudio.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
                launchSeedAudio.start();
                /////Son
                break;
        }
    }

    public void WhenILaunchIMLookingToForwardCam()
    {
        this.orientation = new Vector3(this.cameraReferential.forward.x, 0, this.cameraReferential.forward.z);
    }

    public void AddForce(Vector3 dir, float force)
    {
        dir = dir.normalized;

        dir.y += 0.1f;

        this.direction = dir * force;

        /////Son
        colBalleMe.start();
        /////Son
    }
}
