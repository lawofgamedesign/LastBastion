public class DelayedUpgradeTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the defender who's going to upgrade
	private readonly DefenderSandbox defender;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public DelayedUpgradeTask(DefenderSandbox defender){
		this.defender = defender;
	}


	/// <summary>
	/// Each frame, try to find the last UpgradeTask. When successful, add a task to follow and stop looking.
	/// 
	/// Note that this means this task will keep looking until it finds something. Be careful with starting this task,
	/// because it has the potential to cause unpredicted and unintended behavior.
	/// 
	/// See TurnManager.PlayerUpgrade's OnEnter() for when this task is meant to be used.
	/// </summary>
	public override void Tick (){
		if (Services.Tasks.GetLastTaskOfType<UpgradeTask>() != null){
			Services.Tasks.GetLastTaskOfType<UpgradeTask>().Then(new UpgradeTask(defender));
			SetStatus(TaskStatus.Success);
		}
	}
}
