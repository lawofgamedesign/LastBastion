using UnityEngine;
using UnityEngine.UI;

public class SkilledWarlordBehavior : WarlordSandbox {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//Skilled Warlord stats
	private int skilledSpeed = 1;
	private int skilledAttack = 3;
	private int skilledArmor = 0;
	private int skilledHealth = 2;
	private const string NAME = "Skilled Warlord";


	//animation
	private const string ATTACK_ANIM = "WK_heavy_infantry_07_attack_A";


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
		startHealth = skilledHealth;
		attackerName = NAME;


		//pose the Skilled Warlord
		transform.Find(MODEL_ORGANIZER).Find(MINI_OBJ).GetComponent<Animation>()[ATTACK_ANIM].normalizedTime = 0.3f;
		transform.Find(MODEL_ORGANIZER).Find(MINI_OBJ).GetComponent<Animation>()[ATTACK_ANIM].normalizedSpeed = 0.0f;
		transform.Find(MODEL_ORGANIZER).Find(MINI_OBJ).GetComponent<Animation>().Play();


		//the Skilled Warlord takes a 1 out of the deck when it enters the board, if any are available
		//like the UIManager, this uses a somewhat elaborate system to make sure the tasks get queued correctly
		//tasks are used to avoid undefined resolution orders for AttackerDeck's RemoveCard functions
		if (!Services.Tasks.CheckForTaskOfType<RemoveCardTask>()){ //this is the first attempt to remove a card
			Services.Tasks.AddTask(new RemoveCardTask(transform, 1));
		} else if (Services.Tasks.GetLastTaskOfType<RemoveCardTask>() == null){ //third attempt; can't find the second yet
			Services.Tasks.AddTask(new DelayedRemoveCardTask(transform, 1));
		} else { //second attempt
			Services.Tasks.GetLastTaskOfType<RemoveCardTask>().Then(new RemoveCardTask(transform, 1));
		}
	}
}
