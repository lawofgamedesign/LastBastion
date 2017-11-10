using UnityEngine.UI;

public class EnragedWarlordBehavior : AttackerSandbox {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//Enraged Warlord stats
	private int enragedSpeed = 1;
	private int enragedAttack = 1;
	private int enragedArmor = 0;
	private int enragedHealth = 5;
	private const string NAME = "Enraged Warlord";


	//UI for Enraged Warlord health
	private Image healthUI;
	private const string HEALTH_CANVAS = "Health canvas";
	private const string HEALTH_IMAGE = "Health image";


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
		healthUI = transform.Find(HEALTH_CANVAS).Find(HEALTH_IMAGE).GetComponent<Image>();
		attackerName = NAME;
		Services.AttackDeck.PutCardInDeck(5); //the Enraged Warlord puts a 5 in the deck when it enters the board
	}


	/// <summary>
	/// In addition to normal damage effects, update the Skilled Warlord's health UI.
	/// </summary>
	/// <param name="damage">The amount of damage sustained, after all modifiers.</param>
	public override void TakeDamage (int damage){
		base.TakeDamage(damage);

		healthUI.fillAmount = (float)Health/(float)enragedHealth;
	}
}
