using UnityEngine;

public class RemoveCardTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//information needed to call RemoveCardFromDeck
	private readonly Transform attacker;
	private readonly int value;



	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public RemoveCardTask(Transform attacker, int value){
		this.attacker = attacker;
		this.value = value;
	}


	/// <summary>
	/// Remove a card from the deck, then be done.
	/// </summary>
	public override void Tick (){
		Services.AttackDeck.RemoveCard(attacker, value);
		SetStatus(TaskStatus.Success);
	}
}
