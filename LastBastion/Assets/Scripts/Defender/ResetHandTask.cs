/*
 * 
 * Use this task to queue up resetting a defender's hand.
 * 
 */
public class ResetHandTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the defener whose hand needs to be reset
	private readonly DefenderSandbox defender;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public ResetHandTask(DefenderSandbox defender){
		this.defender = defender;
	}


	/// <summary>
	/// When this task is active, instruct the defender to turn over their cards and then end the task.
	/// </summary>
	public override void Tick(){
		defender.TurnOverAvailableCards();
		SetStatus(TaskStatus.Success);
	}
}
