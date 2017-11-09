using UnityEngine;

public class EjectAttackerTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the attacker to be tossed off the table
	private readonly Rigidbody attacker;


	//the direction in which the attacker is initially moved
	private Vector3 direction = new Vector3(0.0f, 0.0f, 0.0f);
	private const string ATTACKER_PLAYER = "Attacker player";


	//the force of the throw
	private float speed = 30.0f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public EjectAttackerTask(Rigidbody attacker){
		this.attacker = attacker;
	}


	/// <summary>
	/// Find the direction to the attacker player, so that the player can "collect" the defeated attacker.
	/// </summary>
	protected override void Init (){
		direction = (GameObject.Find(ATTACKER_PLAYER).transform.position - attacker.position).normalized;
	}


	/// <summary>
	/// Throw the attacker, and then set this task as complete.
	/// </summary>
	public override void Tick (){
		attacker.AddForce(direction * speed, ForceMode.Impulse);
		SetStatus(TaskStatus.Success);
	}
}
