using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CharacterInputDetector : Bolt.EntityBehaviour<IGuardianState>
{
	[SerializeField] private CompleteCharacterController characterController;
    [SerializeField] private Guardian guardian;

	void Awake()
	{
		if (this.characterController == null) this.characterController = this.GetComponent<CompleteCharacterController>();
	    if (this.guardian == null) this.guardian = this.GetComponent<Guardian>();

	}
	
	// Update is called once per frame
	public void CustomUpdate ()
	{
	    if (!GameSystem.GSystem.EndGame)
	    {
            if (entity.IsOwner)
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
                        this.characterController.UpdateDirection(tmpVec);
                        if (tmpVec != Vector3.zero)
                        {
                            /*if (!this.guardian.IsPreLaunchSeed)
                            {
                                this.characterController.UpdateRotation(tmpVec);
                            }*/
                        }
                    }
                    else
                    {
                        this.characterController.UpdateDirection(Vector3.zero);
                    }

                    if (this.characterController != null && Input.GetButtonDown(InputName.Jump))
                    {
                        if (this.characterController.doubleJumping)
                        {
                            this.characterController.UpdateJump();
                        }
                        else if (!this.characterController.doubleJumping || !this.characterController.Grounded)
                        {
                            this.characterController.UpdateDoubleJump();
                        }


                    }
                }


                #endregion

                #region Attack

                if (this.guardian != null)
                {
                    //if (!this.guardian.IsCooldown && !this.guardian.IsMeleeAttack && !this.guardian.IsLaunchAxe && Input.GetButtonDown(InputName.Bucheronner))
                    {
                        // this.guardian.StartCoroutine(this.guardian.LaunchMeleeAttack());
                    }

                    if (Input.GetButtonDown(InputName.Bucheronner) && !this.guardian.IsLaunchAxe && !this.guardian.IsMeleeAttack)
                    {
                        this.guardian.SetLaunchAxe(true);
                        this.characterController.WhenILaunchIMLookingToForwardCam();
                    }
                    else if (Input.GetButtonDown(InputName.Bucheronner) && this.guardian.IsLaunchAxe && !this.guardian.MyAxe.BackToBucheron)
                    {
                        this.guardian.BackToBucheron();

                    }
                }

                #endregion

                #region SeedLaunch

                if (this.guardian != null)
                {
                    if (!this.guardian.IsCooldown)
                    {
                        if (Input.GetButtonDown(InputName.LancerDeHache))
                        {
                            this.guardian.SetupLaunchSeed();
                        }

                        if (Input.GetButton(InputName.LancerDeHache))
                        {
                            this.guardian.SetupLaunchSeed();
                        }

                        if (Input.GetButtonUp(InputName.LancerDeHache))
                        {
                            this.guardian.LaunchSeed();
                            this.guardian.SetCooldown();
                        }
                    }

                }

                #endregion

            }
        }
       

    }
}
