using UnityEngine;

public class DestroyAttackerTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the attacker to be destroyed
	private readonly GameObject attacker;


	//how long before the attacker is destroyed
	private float delay = 3.0f;
	private float timer = 0.0f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public DestroyAttackerTask(GameObject attacker){
		this.attacker = attacker;
	}


	/// <summary>
	/// Wait until [delay], then destroy the attacker's gameobject.
	/// </summary>
	public override void Tick (){
		timer += Time.deltaTime;

		if (timer >= delay){
			MonoBehaviour.Destroy(attacker);
			SetStatus(TaskStatus.Success);
		}
	}
}
