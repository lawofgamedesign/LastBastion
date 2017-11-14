using UnityEngine;
using UnityEngine.UI;

public class PettyWarlordBehavior : AttackerSandbox {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//petty Warlord stats
	private int pettySpeed = 1;
	private int pettyAttack = 0;
	private int pettyArmor = 0;
	private int pettyHealth = 2;
	private const string NAME = "Petty Warlord";


	//UI for Petty Warlord health
	private Image healthUI;
	private const string HEALTH_CANVAS = "Health canvas";
	private const string HEALTH_IMAGE = "Health image";


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
		healthUI = transform.Find(HEALTH_CANVAS).Find(HEALTH_IMAGE).GetComponent<Image>();
		attackerName = NAME;
	}


	/// <summary>
	/// In addition to normal damage effects, update the Petty Warlord's health UI.
	/// </summary>
	/// <param name="damage">The amount of damage sustained, after all modifiers.</param>
	public override void TakeDamage (int damage){
		base.TakeDamage(damage);

		Debug.Log("Petty Warlord health is now " + Health);

		healthUI.fillAmount = (float)Health/(float)pettyHealth;
	}
}
