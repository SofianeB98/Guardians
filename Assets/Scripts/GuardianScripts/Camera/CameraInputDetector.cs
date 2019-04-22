using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraInputDetector : Bolt.EntityBehaviour<IGuardianState>
{

	[SerializeField] private CameraController cameraController;

	// Update is called once per frame
	public void CustomUpdate () {		
		if (Math.Abs(Input.GetAxis(InputName.MouseHorizontal)) > 0.0f || Math.Abs(Input.GetAxis(InputName.MouseVertical)) > 0.0f ){
			this.cameraController.UpdateAngleManual(new Vector3(
				Input.GetAxis(InputName.MouseVertical),
				Input.GetAxis(InputName.MouseHorizontal),
				0).normalized);
		}
	}
}
