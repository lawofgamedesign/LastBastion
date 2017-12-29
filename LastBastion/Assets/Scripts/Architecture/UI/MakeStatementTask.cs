using UnityEngine;

public class MakeStatementTask : Task {


	private readonly string message;
	private readonly MoveBalloonTask.GrowOrShrink change;


	public MakeStatementTask(string message, MoveBalloonTask.GrowOrShrink change){
		this.message = message;
		this.change = change;
	}


	public override void Tick (){
		if (change == MoveBalloonTask.GrowOrShrink.Grow){
			Services.UI.OpponentStatement(message);
		} else {
			Services.UI.PlayerPhaseStatement(message);
		}

		SetStatus(TaskStatus.Success);
	}
}
