using UnityEngine;
using UnityEngine.UI;

public class PettyWarlordBehavior : WarlordSandbox {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//petty Warlord stats
	private int pettySpeed = 1;
	private int pettyAttack = 0;
	private int pettyArmor = 0;
	private int pettyHealth = 2;
	private const string NAME = "Petty Warlord";





	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables, including the Petty Warlord's stats
	public override void Setup (){
		base.Setup ();
		speed = pettySpeed;
		AttackMod = pettyAttack;
		Armor = pettyArmor;
		Health = pettyHealth;
		startHealth = pettyHealth;
		attackerName = NAME;
	}
}
