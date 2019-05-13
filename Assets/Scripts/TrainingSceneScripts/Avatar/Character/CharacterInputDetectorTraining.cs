using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CharacterInputDetectorTraining : MonoBehaviour
{
    public bool isControllable = true;

    [Header("Base")]
	[SerializeField] private CompleteCharacterControllerTraining characterController;
    [SerializeField] private GuardianTraining guardian;

    [Header("Viseur")]
    [SerializeField] private GameObject viseurStandard;
    [SerializeField] private GameObject viseurSeed;

    private bool seedInput = false;


    void Awake()
	{
	    

        if (this.characterController == null) this.characterController = this.GetComponent<CompleteCharacterControllerTraining>();
	    if (this.guardian == null) this.guardian = this.GetComponent<GuardianTraining>();

	}
	
	// Update is called once per frame
	public void CustomUpdate ()
	{
        if (!this.guardian.IsDie && Time.timeScale > 0.1f)
        {
            //this.guardian.CheckBombSeed();
            this.guardian.CheckVide();
            #region Deplacement

            if (!this.guardian.IsStuned)
            {
                if (this.characterController != null && (Mathf.Abs(Input.GetAxis(InputName.Horizontal)) > 0.1f
                                                         || Mathf.Abs(Input.GetAxis(InputName.Vertical)) > 0.1f) && isControllable)
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
                else if(this.characterController.doubleJumping || this.characterController.jumping || !this.characterController.Grounded)
                {
                    this.characterController.UpdateDirWhenImJumping();
                }
                else
                {
                    this.characterController.UpdateDirection(Vector3.zero);
                }

                if (this.characterController != null && Input.GetButtonDown(InputName.Jump) && this.characterController.Grounded && this.isControllable)
                {
                    this.characterController.UpdateJump();
                }

                if (this.characterController != null && Input.GetButtonDown(InputName.Jump) &&
                    !this.characterController.Grounded && this.isControllable)
                {
                    this.characterController.UpdateDoubleJump();
                }
            }


            #endregion

            #region Attack

            if (this.guardian != null && isControllable)
            {
                //if (!this.guardian.IsCooldown && !this.guardian.IsFusRoDah && !this.guardian.IsLaunchAxe && Input.GetButtonDown(InputName.Bucheronner))
                {
                    // this.guardian.StartCoroutine(this.guardian.LaunchMeleeAttack());
                }

                if (Input.GetButtonDown(InputName.Bucheronner) && !this.guardian.IsFusRoDah) //&& !this.guardian.IsLaunchAxe 
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

            if (this.guardian != null && isControllable)
            {
                if (this.guardian.PillierReadyToLaunch)
                {
                    if (Input.GetButtonDown(InputName.LancerDeHache))
                    {
                        this.guardian.SetupLaunchSeed();
                        this.viseurSeed.SetActive(true);
                        this.viseurStandard.SetActive(false);
                    }

                    if (Input.GetButton(InputName.LancerDeHache))
                    {
                        this.guardian.SetupLaunchSeed();
                        this.viseurSeed.SetActive(true);
                        this.viseurStandard.SetActive(false);
                    }

                    if (Input.GetButtonUp(InputName.LancerDeHache))
                    {
                        this.guardian.LaunchSeed();
                        this.guardian.SetCooldown();

                        this.viseurSeed.SetActive(false);
                        this.viseurStandard.SetActive(true);
                    }
                }
                if (Input.GetAxis(InputName.ChangeSeedSelection) != 0f && !this.seedInput)
                {
                    this.guardian.ChangePillierDir();
                    this.seedInput = true;
                }
                else if (Input.GetAxis(InputName.ChangeSeedSelection) == 0 && this.seedInput)
                {
                    this.seedInput = false;
                }
            }

            #endregion

        }

        if (this.guardian.IsDie)
        {
            this.characterController.UpdateDirection(Vector3.zero);
        }
    }
    
}
