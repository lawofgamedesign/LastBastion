namespace Title
{	
	using UnityEngine;

	public class CameraRotator {

		/////////////////////////////////////////////
		/// Fields
		/////////////////////////////////////////////


		//the object the camera will rotate around
		private Transform centerPoint;
		private const string CENTER_OBJ = "Board center";


		//rotation speed
		private float rotationSpeed = 5.0f; //in degrees/second


		/////////////////////////////////////////////
		/// Functions
		/////////////////////////////////////////////


		//initialize variables
		public void Setup(){
			centerPoint = GameObject.Find(CENTER_OBJ).transform;
		}


		/// <summary>
		/// Rotate the camera around the center of the table each frame.
		/// </summary>
		public void Tick(){
			Camera.main.transform.RotateAround(centerPoint.position, Vector3.up, rotationSpeed * Time.deltaTime);
		}
	}
}
