using System.Collections.Generic;
using UnityEngine;

public class SequencedMoveTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the attackers who might move
	private List<AttackerSandbox> attackers = new List<AttackerSandbox>();


	//which attacker is attempting to move
	private int index = 0;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//Get the attackers, and then start them trying to move. 
	protected override void Init (){
		attackers = Services.Attackers.GetAttackers();
		Services.Events.Register<SequencedMoveEvent>(GoToNextAttacker);
		OrderNextMove();
	}


	/// <summary>
	/// Unregister for events.
	/// </summary>
	protected override void Cleanup (){
		Services.Events.Unregister<SequencedMoveEvent>(GoToNextAttacker);
	}


	/// <summary>
	/// Tell an attacker to try to move.
	/// </summary>
	private void OrderNextMove(){
		Debug.Assert(index < attackers.Count, "Trying to move a non-existent attacker");

		attackers[index].TryMove();
	}


	/// <summary>
	/// Tell the next attacker to move, or be done if there are no more attackers.
	/// </summary>
	/// <param name="e">A SequencedMoveEvent.</param>
	private void GoToNextAttacker(global::Event e){
		Debug.Assert(e.GetType() == typeof(SequencedMoveEvent), "Non-SequencedMoveEvent in GoToNextAttacker");

		index++;

		if (index <= attackers.Count) OrderNextMove();
		else SetStatus(TaskStatus.Success);
	}
}
