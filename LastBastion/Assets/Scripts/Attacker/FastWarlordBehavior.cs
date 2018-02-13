using UnityEngine; //needed to serialize fields
using UnityEngine.UI;

public class FastWarlordBehavior : WarlordSandbox {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//Fast Warlord stats
	private int fastSpeed = 2;
	private int fastAttack = 2;
	private int fastArmor = 0;
	private int fastHealth = 2;
	private const string NAME = "Fast Warlord";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables, including the Fast Warlord's stats
	public override void Setup (){
		base.Setup ();
		speed = fastSpeed;
		AttackMod = fastAttack;
		Armor = fastArmor;
		Health = fastHealth;
		startHealth = fastHealth;
		attackerName = NAME;
	}
}
