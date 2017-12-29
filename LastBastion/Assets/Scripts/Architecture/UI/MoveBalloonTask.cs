using TMPro;
using UnityEngine;

public class MoveBalloonTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the balloon that will move
	private readonly RectTransform balloon;
	private const string BALLOON_OBJ = "Speech balloon";
	private const string CHAT_UI_ORGANIZER = "Chat UI";


	//the text in the balloon
	private readonly string message;
	private readonly TextMeshProUGUI balloonText;
	private const string TEXT_OBJ = "Text";


	//where the balloon is going
	private Vector3 targetLoc;
	private Vector3 direction;
	private const string BALLOON_TARGET = "Speech balloon target";


	//speed
	private float speed = 500.0f;


	//balloon starting location
	private float startDistance;


	//is the balloon growing or shrinking?
	public enum GrowOrShrink { Grow, Shrink };
	private readonly GrowOrShrink change;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public MoveBalloonTask(Vector3 position, string message, GrowOrShrink change){
		RectTransform balloon = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(BALLOON_OBJ),
																	  GameObject.Find(CHAT_UI_ORGANIZER).transform).GetComponent<RectTransform>();
		balloon.transform.position = position;

		this.balloon = balloon;

		this.message = message;

		balloon.transform.Find(TEXT_OBJ).GetComponent<TextMeshProUGUI>().text = this.message;

		this.change = change;
	}


	/// <summary>
	/// Determine the direction in which the speech balloon is moving, and the total distance to move.
	/// </summary>
	protected override void Init (){
		targetLoc = GameObject.Find(BALLOON_TARGET).transform.position;
		direction = (targetLoc - balloon.transform.position).normalized;
		startDistance = Vector3.Distance(balloon.transform.position, targetLoc);
	}


	/// <summary>
	/// Move the speech balloon each frame, shrinking it as it goes, until it arrives at the chat bar.
	/// </summary>
	public override void Tick (){
		balloon.localScale = ResizeBalloon();

		bool arrived = false;

		balloon.transform.position = MoveBalloon(out arrived);

		if (arrived) SetStatus(TaskStatus.Success);
	}


	/// <summary>
	/// Shrink the balloon based on how close it is to the chat window.
	/// </summary>
	/// <returns>The balloon's scale this frame.</returns>
	private Vector3 ResizeBalloon(){

		if (change == GrowOrShrink.Shrink){
			Vector3 newScale = new Vector3(1.0f, 1.0f, 1.0f);

			float multiplier = Vector3.Distance(balloon.transform.position, targetLoc)/startDistance;

			newScale *= multiplier;

			return newScale;
		} else {
			float newScale = 1.0f - Vector3.Distance(balloon.transform.position, targetLoc)/startDistance;

			return new Vector3(newScale, newScale, newScale);
		}
	}


	/// <summary>
	/// Move the balloon toward the bottom of the chat window.
	/// </summary>
	/// <returns>The balloon's position this frame.</returns>
	/// <param name="done">Has the balloon reached its destination?</param>
	private Vector3 MoveBalloon(out bool done){
		done = false;

		if (Vector3.Distance(balloon.transform.position, targetLoc) <= speed * Time.deltaTime){
			done = true;
			return targetLoc;
		} else {
			return balloon.transform.position + (direction * speed * Time.deltaTime);
		}
	}


	/// <summary>
	/// Get rid of the speech balloon when it reaches the chat window
	/// </summary>
	protected override void OnSuccess (){
		Services.UI.MakeStatement(message);
		MonoBehaviour.Destroy(balloon.gameObject);
	}
}
