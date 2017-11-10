using UnityEngine.UI;

public class SkilledWarlordBehavior : AttackerSandbox {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//Skilled Warlord stats
	private int skilledSpeed = 1;
	private int skilledAttack = 3;
	private int skilledArmor = 0;
	private int skilledHealth = 2;
	private const string NAME = "Skilled Warlord";


	//UI for Skilled Warlord health
	private Image healthUI;
	private const string HEALTH_CANVAS = "Health canvas";
	private const string HEALTH_IMAGE = "Health image";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables, including the Skilled Warlord's stats
	public override void Setup (){
		base.Setup ();
		speed = skilledSpeed;
		AttackMod = skilledAttack;
		Armor = skilledArmor;
		Health = skilledHealth;
		healthUI = transform.Find(HEALTH_CANVAS).Find(HEALTH_IMAGE).GetComponent<Image>();
		attackerName = NAME;
		Services.AttackDeck.RemoveCardFromDeck(1); //the Skilled Warlord takes a 1 out of the deck when it enters the board, if any are available
	}


	/// <summary>
	/// In addition to normal damage effects, update the Skilled Warlord's health UI.
	/// </summary>
	/// <param name="damage">The amount of damage sustained, after all modifiers.</param>
	public override void TakeDamage (int damage){
		base.TakeDamage(damage);

		healthUI.fillAmount = (float)Health/(float)skilledHealth;
	}
}
