using UnityEngine.UI;

public class ArmoredWarlordBehavior : WarlordSandbox {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//armored warlord stats
	private int armoredSpeed = 1;
	private int armoredAttack = 0;
	private int armoredArmor = 3;
	private int armoredHealth = 4;
	private const string NAME = "Armored Warlord";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables, including the Armored Warlord's stats
	public override void Setup (){
		speed = armoredSpeed;
		AttackMod = armoredAttack;
		Armor = armoredArmor;
		Health = armoredHealth;
		startHealth = armoredHealth;
		attackerName = NAME;
		base.Setup ();
	}
}
