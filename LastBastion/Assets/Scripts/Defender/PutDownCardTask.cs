using UnityEngine;

public class PutDownCardTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	private readonly RectTransform cardTransform;
	private readonly Vector3 dropSpeed = new Vector3(0.0f, -10.0f, 0.0f);
	private readonly DefenderUIBehavior uICanvas;
	private const string UI_CANVAS = "Defender UI canvas";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public PutDownCardTask(RectTransform cardTransform){
		this.cardTransform = cardTransform;
		uICanvas = GameObject.Find(UI_CANVAS).GetComponent<DefenderUIBehavior>();
	}


	/// <summary>
	/// Each frame, drop the card until it reaches its starting height.
	/// 
	/// This assumes that the canvas is at the starting height!
	/// </summary>
	public override void Tick(){
		cardTransform.position += dropSpeed * Time.deltaTime;

		if (cardTransform.localPosition.z >= 0.0f) SetStatus(TaskStatus.Success); //here again, z is effectively the vertical axis
	}


	/// <summary>
	/// If this is the last card a defender used, such that they are no longer selected, switch the cards off.
	/// </summary>
	protected override void Cleanup (){
		if (!Services.Defenders.IsAnyoneSelected()) uICanvas.ShutCardsOff();
	}
}
