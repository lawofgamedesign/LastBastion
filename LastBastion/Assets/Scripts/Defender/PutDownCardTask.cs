using UnityEngine;

public class PutDownCardTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	private readonly RectTransform cardTransform;
	private readonly Vector3 dropSpeed = new Vector3(0.0f, -10.0f, 0.0f);
	private readonly DefenderUIBehavior uICanvas;
	private const string UI_CANVAS = "Defender card canvas";


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
		if (cardTransform.position.y + dropSpeed.y * Time.deltaTime <= uICanvas.transform.position.y) {
			cardTransform.position = new Vector3(cardTransform.position.x,
												 uICanvas.transform.position.y,
												 cardTransform.position.z);
			SetStatus(TaskStatus.Success);
		} else {
			cardTransform.position += dropSpeed * Time.deltaTime;
		}
	}
}
