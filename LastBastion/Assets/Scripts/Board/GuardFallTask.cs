/// <summary>
/// Juice for the walls losing strength. Causes a chosen guard to fall from the wall.
/// </summary>
using UnityEngine;

public class GuardFallTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the guard that will fall from the wall
	private readonly Rigidbody guard;

	//the speed with which the guard tips over, in degrees/second
	private float rotationSpeed = 180.0f;

	//the guard's final rotation
	private Quaternion fallenRotation;

	//the speed with which the guard falls
	private float fallSpeed = 4.5f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public GuardFallTask(Rigidbody guard){
		this.guard = guard;
		fallenRotation = Quaternion.Euler(new Vector3(-90.0f, 0.0f, 0.0f));
	}


	/// <summary>
	/// Each frame, tip the guard over until it's fallen off the wall, then drop it to the ground.
	/// 
	/// This task is done when the guard reaches the ground.
	/// </summary>
	public override void Tick(){
		if (Quaternion.Angle(guard.rotation, fallenRotation) > Mathf.Epsilon){
			guard.MoveRotation(Quaternion.RotateTowards(guard.rotation, fallenRotation, rotationSpeed * Time.deltaTime));
		} else {
			SetStatus(TaskStatus.Success);
			//use this if the guard is to fall, as opposed to merely tipping over
			//guard.MovePosition(guard.position + -Vector3.up * fallSpeed * Time.deltaTime);
		}

		//use this if the guard is to fall, as opposed to merely tipping over
		//if (guard.position.y <= 0.0f) SetStatus(TaskStatus.Success);
	}
}
