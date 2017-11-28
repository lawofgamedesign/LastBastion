using UnityEngine;

public class DelayedPutInCardTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//information to be passed to a PutInCardTask
	private readonly Transform endLoc;
	private readonly int value;


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//constructor
	public DelayedPutInCardTask(Transform endLoc, int value){
		this.endLoc = endLoc;
		this.value = value;
	}


	/// <summary>
	/// Each frame, try to find the last PutInCardTask. When successful, add a task to follow and stop looking.
	/// 
	/// Note that this means this task will keep looking until it finds something. Be careful with starting this task,
	/// because it has the potential to cause unpredicted and unintended behavior.
	/// 
	/// See UIManager's AddCardToDeck() for when this task is meant to be used.
	/// </summary>
	public override void Tick (){
		if (Services.Tasks.GetLastTaskOfType<PutInCardTask>() != null){
			Services.Tasks.GetLastTaskOfType<PutInCardTask>().Then(new PutInCardTask(endLoc, value));
			SetStatus(TaskStatus.Success);
		}
	}
}
