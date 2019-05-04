using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraInputDetectorTraining : Bolt.EntityBehaviour<IGuardianState>
{

	[SerializeField] private CameraControllerTraining cameraController;

	// Update is called once per frame
	public void CustomUpdate () {

	    if (Time.timeScale > 0.1f)
	    {
	        if (Math.Abs(Input.GetAxis(InputName.MouseHorizontal)) > 0.1f || Math.Abs(Input.GetAxis(InputName.MouseVertical)) > 0.1f)
	        {
	            this.cameraController.UpdateAngleManual(new Vector3(
	                Input.GetAxis(InputName.MouseVertical),
	                Input.GetAxis(InputName.MouseHorizontal),
	                0));
	        }
        }
		
	}
}
