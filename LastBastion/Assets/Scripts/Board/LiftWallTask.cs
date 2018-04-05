using UnityEngine;

public class LiftWallTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the wall that will be lifted out of the way
	private readonly Transform wall;


	//how high the wall moves
	private const float RAISED_HEIGHT = 3.5f;


	//the speed with which the wall rises and lowers
	private readonly Vector3 speed = new Vector3(0.0f, 25.0f, 0.0f);


	//is the wall rising or falling?
	public enum UpOrDown { Up = 1, Down = -1 }
	private UpOrDown currentAction;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public LiftWallTask(Transform wall, UpOrDown currentAction){
		this.wall = wall;
		this.currentAction = currentAction;
	}


	public override void Tick (){
		wall.position += speed * (int)currentAction * Time.deltaTime;

		if (currentAction == UpOrDown.Up && wall.position.y >= RAISED_HEIGHT) SetStatus(TaskStatus.Success);
		else if (currentAction == UpOrDown.Down && wall.position.y <= 0.0f) SetStatus(TaskStatus.Success);
	}


	/// <summary>
	/// Make sure the wall is lowered to the correct height, if this task is lowering the wall.
	/// </summary>
	protected override void Cleanup (){
		if (currentAction == UpOrDown.Down){
			wall.position = new Vector3(wall.position.x, 0.0f, wall.position.z);
		}
	}
}
