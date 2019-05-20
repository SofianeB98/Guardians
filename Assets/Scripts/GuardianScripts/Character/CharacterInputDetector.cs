using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CharacterInputDetector : Bolt.EntityBehaviour<IGuardianState>
{
    [Header("Base")]
	[SerializeField] private CompleteCharacterController characterController;
    [SerializeField] private Guardian guardian;

    [Header("Viseur")]
    [SerializeField] private GameObject viseurStandard;
    [SerializeField] private GameObject viseurSeed;

    private bool seedInput = false;
    
    void Awake()
	{
        if (this.characterController == null) this.characterController = this.GetComponent<CompleteCharacterController>();
	    if (this.guardian == null) this.guardian = this.GetComponent<Guardian>();

	}
	
	// Update is called once per frame
	public void CustomUpdate ()
	{
	    if (!GameSystem.GSystem.EndGame && GameSystem.GSystem.GameStart)
	    {
            if (entity.IsOwner && !this.guardian.IsDie)
            {
                //this.guardian.CheckBombSeed();
                this.guardian.CheckVide();
                #region Deplacement

                if (!this.guardian.IsStuned)
                {
                    if (this.characterController != null && (Mathf.Abs(Input.GetAxis(InputName.Horizontal)) > 0.1f
                                                             || Mathf.Abs(Input.GetAxis(InputName.Vertical)) > 0.1f))
                    {
                        var tmpVec = new Vector3(Input.GetAxis(InputName.Horizontal), 0, Input.GetAxis(InputName.Vertical)).normalized;
                        state.xInput = Input.GetAxis(InputName.Horizontal);
                        state.yInput = Input.GetAxis(InputName.Vertical);
                        this.characterController.UpdateDirection(tmpVec);
                        if (tmpVec != Vector3.zero)
                        {
                            /*if (!this.guardian.IsPreLaunchSeed)
                            {
                                this.characterController.UpdateRotation(tmpVec);
                            }*/
                        }
                    }
                    else if (this.characterController.jumping || this.characterController.doubleJumping)
                    {
                        this.characterController.UpdateDirWhenImJumping();
                        state.xInput = 0;
                        state.yInput = 0;
                    }
                    else
                    {
                        this.characterController.UpdateDirection(Vector3.zero);
                        state.xInput = 0;
                        state.yInput = 0;
                    }
                    
                }

                if (this.characterController != null && Input.GetButtonDown(InputName.Jump) && this.characterController.Grounded)
                {
                    this.characterController.UpdateJump();
                }

                if (this.characterController != null && Input.GetButtonDown(InputName.Jump) &&
                    !this.characterController.Grounded)
                {
                    this.characterController.UpdateDoubleJump();
                }

                #endregion

                #region Attack

                if (this.guardian != null)
                {
                    //if (!this.guardian.IsCooldown && !this.guardian.IsFusRoDah && !this.guardian.IsLaunchAxe && Input.GetButtonDown(InputName.Bucheronner))
                    {
                        // this.guardian.StartCoroutine(this.guardian.LaunchMeleeAttack());
                    }

                    if (Input.GetButtonDown(InputName.Bucheronner) && !this.guardian.IsLaunchAxe && !this.guardian.IsFusRoDah)
                    {
                        //this.guardian.SetLaunchAxe(true);
                        this.guardian.FusRoDa();
                        this.characterController.WhenILaunchIMLookingToForwardCam();
                    }
                    //else if (Input.GetButtonDown(InputName.Bucheronner) && this.guardian.IsLaunchAxe && !this.guardian.MyAxe.BackToBucheron)
                    //{
                        //this.guardian.BackToBucheron();

                    //}
                }

                #endregion

                #region SeedLaunch

                if (this.guardian != null)
                {

                    if (Input.GetButtonDown(InputName.LancerDeHache))
                    {
                        if (this.guardian.PillierReadyToLaunch)
                        {
                            this.guardian.SetupLaunchSeed();
                        }
                        this.viseurSeed.SetActive(true);
                        this.viseurStandard.SetActive(false);
                    }

                    if (Input.GetButton(InputName.LancerDeHache))
                    {
                        if (this.guardian.PillierReadyToLaunch)
                        {
                            this.guardian.SetupLaunchSeed();
                        }
                        
                        this.viseurSeed.SetActive(true);
                        this.viseurStandard.SetActive(false);
                    }

                    if (Input.GetButtonUp(InputName.LancerDeHache))
                    {
                        if (this.guardian.PillierReadyToLaunch)
                        {
                            this.guardian.LaunchSeed();
                            this.guardian.SetCooldown();
                        }
                        this.viseurSeed.SetActive(false);
                        this.viseurStandard.SetActive(true);
                    }
                    
                    /*if (Input.GetAxis(InputName.ChangeSeedSelection) != 0f && !this.seedInput)
                    {
                        this.guardian.ChangePillierDir();
                        this.seedInput = true;
                    }
                    else if (Input.GetAxis(InputName.ChangeSeedSelection) == 0 && this.seedInput)
                    {
                        this.seedInput = false;
                    }*/
                }

                #endregion

            }

	        if (this.guardian.IsDie)
	        {
	            this.characterController.UpdateDirection(Vector3.zero);
            }
        }
	    else
	    {
	        this.characterController.UpdateDirection(Vector3.zero);
	    }

    }
}
