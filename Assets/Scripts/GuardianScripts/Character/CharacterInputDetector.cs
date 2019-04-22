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
	    if (entity.IsOwner)
	    {
	        if (!this.guardian.IsStuned)
	        {
	            #region Deplacement

	            if (this.characterController != null && (Input.GetButton(InputName.Horizontal) || Input.GetButton(InputName.Vertical)))
	            {
	                var tmpVec = new Vector3(Input.GetAxis(InputName.Horizontal), 0, Input.GetAxis(InputName.Vertical)).normalized;
	                this.characterController.UpdateDirection(tmpVec);
	                if (tmpVec != Vector3.zero) this.characterController.UpdateRotation(tmpVec);
	            }
	            else
	            {
	                this.characterController.UpdateDirection(Vector3.zero);
	            }

	            if (this.characterController != null && Input.GetButtonDown(InputName.Jump))
	            {
	                this.characterController.UpdateJump();
	            }

	            #endregion

	            #region Attack

	            if (this.guardian != null)
	            {
	                if (!this.guardian.IsCooldown && !this.guardian.IsMeleeAttack && Input.GetButtonDown(InputName.Bucheronner))
	                {
	                    this.guardian.StartCoroutine(this.guardian.LaunchMeleeAttack());
	                }
	            }

	            #endregion

	            #region SeedLaunch

	            if (this.guardian != null)
	            {
	                if (Input.GetButtonUp(InputName.SeedLaunch))
	                {
	                    this.guardian.LaunchSeed();
	                }
	            }

	            #endregion
	        }
        }
	    
    }
}
