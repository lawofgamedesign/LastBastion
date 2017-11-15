using UnityEngine; //needed to serialize fields
using UnityEngine.UI;

public class FastWarlordBehavior : AttackerSandbox {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//Fast Warlord stats
	private int fastSpeed = 2;
	private int fastAttack = 2;
	private int fastArmor = 0;
	private int fastHealth = 2;
	private const string NAME = "Fast Warlord";


	//UI for Petty Warlord health
	private Image healthUI;
	private const string HEALTH_CANVAS = "Health canvas";
	private const string HEALTH_IMAGE = "Health image";


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
		healthUI = transform.Find(HEALTH_CANVAS).Find(HEALTH_IMAGE).GetComponent<Image>();
		attackerName = NAME;
	}


	/// <summary>
	/// In addition to normal damage effects, update the Petty Warlord's health UI.
	/// </summary>
	/// <param name="damage">The amount of damage sustained, after all modifiers.</param>
	public override void TakeDamage (int damage){
		base.TakeDamage(damage);

		healthUI.fillAmount = (float)Health/(float)fastHealth;
	}
}
