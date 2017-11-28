/// <summary>
/// A trivial task for delaying actions by a frame.
/// </summary>
public class WaitTask : Task {

	public WaitTask() {
		//intentionally blank
	}


	public override void Tick (){
		SetStatus(TaskStatus.Success);
	}
}
