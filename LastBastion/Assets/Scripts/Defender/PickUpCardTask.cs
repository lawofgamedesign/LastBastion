using UnityEngine;

public class PickUpCardTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	private readonly RectTransform cardTransform;
	private readonly Vector3 liftSpeed = new Vector3(0.0f, 10.0f, 00.0f);
	private readonly Vector3 startLoc;
	private readonly float totalLift = 5.0f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public PickUpCardTask(RectTransform cardTransform){
		this.cardTransform = cardTransform;
		startLoc = this.cardTransform.position;
	}


	/// <summary>
	/// Each frame, lift the card until it reaches the appointed height.
	/// </summary>
	public override void Tick(){
		cardTransform.position += liftSpeed * Time.deltaTime;

		if (Vector3.Distance(startLoc, cardTransform.position) >= totalLift) SetStatus(TaskStatus.Success);
	}
}
