public class WaitForSecondsTask : Task {


	private readonly float waitTime;
	private float timer = 0.0f;


	public WaitForSecondsTask(float waitTime){
		this.waitTime = waitTime;
	}


	public override void Tick (){
		timer += UnityEngine.Time.deltaTime;

		if (timer >= waitTime) SetStatus(TaskStatus.Success);
	}
}
