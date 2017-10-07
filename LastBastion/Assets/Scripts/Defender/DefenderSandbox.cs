/// <summary>
/// Base class for defenders. All defender "verbs" are contained here.
/// </summary>
using UnityEngine;

public class DefenderSandbox : MonoBehaviour {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//defender stats
	protected int speed; //in spaces/turn
	protected int attackMod;
	protected int armor;


	//generic stats for testing purposes
	private int baseSpeed = 4;
	private int baseAttackMod = 1;
	private int baseArmor = 1;


	protected virtual void Setup(){
		speed = baseSpeed;
		attackMod = baseAttackMod;
		armor = baseArmor;
	}
}
