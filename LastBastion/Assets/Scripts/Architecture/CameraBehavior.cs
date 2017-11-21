using UnityEngine;

public class CameraBehavior : MonoBehaviour {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the camera's position and rotation when able to see the entire board
	private Vector3 overviewPos = new Vector3(20.0f, 24.11f, -4.0f);
	private Vector3 overviewRot = new Vector3(58.97f, 0.0f, 0.0f);


	//the camera's current position and rotation as it scrolls around
	private Vector3 currentPos = new Vector3(0.0f, 0.0f, 0.0f);
	private Vector3 currentRot = new Vector3(0.0f, 0.0f, 0.0f);
}
