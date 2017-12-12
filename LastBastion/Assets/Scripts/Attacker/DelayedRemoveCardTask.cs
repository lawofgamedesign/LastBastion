using UnityEngine;

public class DelayedRemoveCardTask : Task {

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
	public DelayedRemoveCardTask(Transform attacker, int value){
		this.attacker = attacker;
		this.value = value;
	}


	/// <summary>
	/// Each frame, try to find the last RemoveCardTask. When successful, tack on another RemoveCardTask, then be done.
	/// This resolves the situation wherein there's two RemoveCardTasks, and the system wants to set up a third, keeping
	/// the tasks in order.
	/// 
	/// Note that this means this task will keep looking until it finds something. Be careful with starting this task,
	/// because it has the potential to cause unpredicted and unintended behavior.
	/// </summary>
	public override void Tick (){
		if (Services.Tasks.GetLastTaskOfType<RemoveCardTask>() != null){
			Services.Tasks.GetLastTaskOfType<RemoveCardTask>().Then(new RemoveCardTask(attacker, value));
			SetStatus(TaskStatus.Success);
		}
	}
}
