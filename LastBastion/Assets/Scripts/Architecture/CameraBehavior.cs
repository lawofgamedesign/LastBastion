using UnityEngine;

public class CameraBehavior {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//speed with which the player's view rotates
	private float rotSpeed = 50.0f;


	//the camera's default rotation
	private Vector3 tableAngle = new Vector3(66.4f, 0.0f, 0.0f);
	private Quaternion lookAtTable;


	//rotation when looking up
	private Vector3 upAngle = new Vector3(28.3f, 0.0f, 0.0f);
	private Quaternion lookUp;


	//rotation when looking left
	private Vector3 leftAngle = new Vector3(28.3f, -59.7f, 0.0f);
	private Quaternion lookLeft;


	//rotation when looking right
	private Vector3 rightAngle = new Vector3(28.3f, 59.7f, 0.0f);
	private Quaternion lookRight;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	public void Setup(){
		lookAtTable = Quaternion.Euler(tableAngle);
		lookUp = Quaternion.Euler(upAngle);
		lookLeft = Quaternion.Euler(leftAngle);
		lookRight = Quaternion.Euler(rightAngle);
	}


	/// <summary>
	/// Redirect the camera toward the table, if it's not there already.
	/// </summary>
	public void CameraToTable(){
		if (Camera.main.transform.rotation != lookAtTable){
			Camera.main.transform.rotation = Quaternion.RotateTowards(Camera.main.transform.rotation, lookAtTable, rotSpeed * Time.deltaTime);
		}
	}


	/// <summary>
	/// Direct the camera to look "up" (toward the opponent).
	/// </summary>
	public void CameraUp(){
		Camera.main.transform.rotation = Quaternion.RotateTowards(Camera.main.transform.rotation, lookUp, rotSpeed * Time.deltaTime);
	}


	/// <summary>
	/// Direct the camera to look "left" (treating looking at the table as forward).
	/// </summary>
	public void CameraLeft(){
		Camera.main.transform.rotation = Quaternion.RotateTowards(Camera.main.transform.rotation, lookLeft, rotSpeed * Time.deltaTime);
	}


	/// <summary>
	/// Direct the camera to look "right" (treating looking at the table as forward).
	/// </summary>
	public void CameraRight(){
		Camera.main.transform.rotation = Quaternion.RotateTowards(Camera.main.transform.rotation, lookRight, rotSpeed * Time.deltaTime);
	}
}
