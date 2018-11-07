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


	//the extra die for the Enraged Warlord's health
	private Transform healthSpindown2;
	private const string HEALTH_SPINDOWN_ADDL_OBJ = "Health spindown 2";
	private const int MAX_D6_VALUE = 6;


	//animation
	private const string ATTACK_ANIM = "WK_heavy_infantry_07_attack_A";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables, including the Enraged Warlord's stats
	public override void Setup (){
		speed = enragedSpeed;
		AttackMod = enragedAttack;
		Armor = enragedArmor;
		Health = enragedHealth;
		startHealth = enragedHealth;
		attackerName = NAME;
		healthSpindown2 = transform.Find(HEALTH_SPINDOWN_ADDL_OBJ);
		base.Setup ();


		//pose the Enraged Warlord
		transform.Find(MODEL_ORGANIZER).Find(MINI_OBJ).GetComponent<Animation>()[ATTACK_ANIM].normalizedTime = 0.5f;
		transform.Find(MODEL_ORGANIZER).Find(MINI_OBJ).GetComponent<Animation>()[ATTACK_ANIM].normalizedSpeed = 0.0f;
		transform.Find(MODEL_ORGANIZER).Find(MINI_OBJ).GetComponent<Animation>().Play();


		//the Enraged Warlord puts a 5 in the deck when it enters the board
		Services.AttackDeck.AddCard(transform, 5);
	}


	/// <summary>
	/// The Enraged Warlord has more health than other warlords, and thus must use 2d6.
	/// </summary>
	/// <param name="health">The num.</param>
	protected override void SpinDie(int health){
		Debug.Assert(health > 0, "Trying to spin down to zero");
		Debug.Assert(health < 8, "Trying to spin above 7");

		//if setting the Enraged Warlord's initial health, handle both dice
		if (health == enragedHealth){
			healthSpindown.rotation = sides[MAX_D6_VALUE - 1];
			healthSpindown.Rotate(Vector3.up, Random.Range(MIN_Y_ROT, MAX_Y_ROT), Space.World); //spin the die around the Y-axis, to make it look more naturally placed
			healthSpindown2.Rotate(Vector3.up, Random.Range(MIN_Y_ROT, MAX_Y_ROT), Space.World); //spin the die around the Y-axis, to make it look more naturally placed
		} else { //if the health is lower than initial health, shut off the extra die and handle the die all warlords have normally
			healthSpindown2.gameObject.SetActive(false);
			healthSpindown.rotation = sides[health - 1]; //-1 because the list is zero-indexed
			healthSpindown.Rotate(Vector3.up, Random.Range(MIN_Y_ROT, MAX_Y_ROT), Space.World); //spin the die around the Y-axis, to make it look more naturally placed
		}
	}
}
