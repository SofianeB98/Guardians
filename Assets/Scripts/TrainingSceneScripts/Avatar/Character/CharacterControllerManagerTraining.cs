using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterControllerManagerTraining : MonoBehaviour
{
    public bool IsControllable = true;

	[SerializeField]
	private CompleteCharacterControllerTraining characterController;
	[SerializeField]
	private CharacterInputDetectorTraining characterInput;
	[SerializeField]
	private CameraControllerTraining cameraController;
	[SerializeField]
	private CameraInputDetectorTraining cameraInput;
	
	// Use this for initialization
	void Awake() {
		if (this.characterController == null){
			this.characterController = GetComponent<CompleteCharacterControllerTraining>();
		}
		if (this.characterInput == null){
			this.characterInput = GetComponent<CharacterInputDetectorTraining>();
		}
		if (this.cameraController == null){
			this.cameraController = GetComponent<CameraControllerTraining>();
		}
		if (this.cameraInput == null){
			this.cameraInput = GetComponent<CameraInputDetectorTraining>();
		}
	    Cursor.lockState = CursorLockMode.Locked;
	    Cursor.visible = false;
	}

    // Update is called once per frame
    private void Update()
    {
        if (this.characterInput != null)
        {
            this.characterInput.CustomUpdate();
        }

        if (IsControllable)
        {
            
            if (this.cameraInput != null)
            {
                this.cameraInput.CustomUpdate();
            }
            
            
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
