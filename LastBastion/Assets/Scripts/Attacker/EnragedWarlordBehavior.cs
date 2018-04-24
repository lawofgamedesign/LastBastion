using UnityEngine;
using UnityEngine.UI;

public class EnragedWarlordBehavior : WarlordSandbox {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//Enraged Warlord stats
	private int enragedSpeed = 1;
	private int enragedAttack = 0;
	private int enragedArmor = 0;
	private int enragedHealth = 7;
	private const string NAME = "Enraged Warlord";


	//animation
	private const string ATTACK_ANIM = "WK_heavy_infantry_07_attack_A";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables, including the Enraged Warlord's stats
	public override void Setup (){
		base.Setup ();
		speed = enragedSpeed;
		AttackMod = enragedAttack;
		Armor = enragedArmor;
		Health = enragedHealth;
		startHealth = enragedHealth;
		attackerName = NAME;


		//pose the Enraged Warlord
		transform.Find(MODEL_ORGANIZER).Find(MINI_OBJ).GetComponent<Animation>()[ATTACK_ANIM].normalizedTime = 0.5f;
		transform.Find(MODEL_ORGANIZER).Find(MINI_OBJ).GetComponent<Animation>()[ATTACK_ANIM].normalizedSpeed = 0.0f;
		transform.Find(MODEL_ORGANIZER).Find(MINI_OBJ).GetComponent<Animation>().Play();


		//the Enraged Warlord puts a 5 in the deck when it enters the board
		Services.AttackDeck.AddCard(transform, 5);
	}
}
