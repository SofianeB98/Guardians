﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllerManager : Bolt.EntityBehaviour<IGuardianState>
{
	[SerializeField]
	private CompleteCharacterController characterController;
	[SerializeField]
	private CharacterInputDetector characterInput;
	[SerializeField]
	private CameraController cameraController;
	[SerializeField]
	private CameraInputDetector cameraInput;
	
	// Use this for initialization
	void Awake() {
		if (this.characterController == null){
			this.characterController = GetComponent<CompleteCharacterController>();
		}
		if (this.characterInput == null){
			this.characterInput = GetComponent<CharacterInputDetector>();
		}
		if (this.cameraController == null){
			this.cameraController = GetComponent<CameraController>();
		}
		if (this.cameraInput == null){
			this.cameraInput = GetComponent<CameraInputDetector>();
		}
	    Cursor.lockState = CursorLockMode.Locked;
	    Cursor.visible = false;
	}

    // Update is called once per frame
    public override void SimulateOwner()
    {
        if (this.characterInput != null)
        {
            this.characterInput.CustomUpdate();
        }
        if (this.cameraInput != null)
        {
            this.cameraInput.CustomUpdate();
        }
        if (this.characterController != null)
        {
            this.characterController.CustomUpdate();
        }
        if (this.cameraController != null)
        {
            this.cameraController.CustomUpdate();
        }
    }
}
