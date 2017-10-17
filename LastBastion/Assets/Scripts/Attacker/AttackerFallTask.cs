using UnityEngine;

public class AttackerFallTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the falling attacker
	private readonly Rigidbody attacker;


	//the speed with which the attacker tips over, in degrees/second
	private float rotationSpeed = 180.0f;


	//the attacker's final rotation
	private Quaternion fallenRotation;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public AttackerFallTask(Rigidbody attacker){
		this.attacker = attacker;
		fallenRotation = Quaternion.Euler(123.0f, 0.0f, 0.0f);
	}


	/// <summary>
	/// Each frame, tip the attacker backwards.
	/// </summary>
	public override void Tick(){
		if (Quaternion.Angle(attacker.rotation, fallenRotation) > Mathf.Epsilon){
			attacker.MoveRotation(Quaternion.RotateTowards(attacker.rotation, fallenRotation, rotationSpeed * Time.deltaTime));
		} else {
			SetStatus(TaskStatus.Success);
		}
	}
}
