using UnityEngine;

public class SelectIconMoveTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the defender whose selectable icon is going to be moving
	private readonly DefenderSandbox defender;


	//the icon that moves
	private readonly Transform selectableIcon;
	private const string PRIVATE_UI_CANVAS = "Defender canvas";
	private const string SELECTABLE_ICON_OBJ = "Selectable icon";


	//the positions the icon moves between
	private Vector3 startPos = new Vector3(40.0f, -45.0f, -1.0f);
	private Vector3 endPos = new Vector3(45.0f, -50.0f, -1.0f);


	//an animation curve used to move the icon
	private AnimationCurve moveCurve = new AnimationCurve(new Keyframe(0.0f, 0.0f), new Keyframe(0.5f, 1.0f), new Keyframe(1.0f, 0.0f));
	private float timer = 0.0f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public SelectIconMoveTask(DefenderSandbox defender){
		this.defender = defender;

		selectableIcon = this.defender.transform.Find(PRIVATE_UI_CANVAS).Find(SELECTABLE_ICON_OBJ);

		Services.Events.Register<NotSelectableEvent>(ListenForDefenderDone);
		Services.Events.Register<PhaseStartEvent>(ListenForCombatEnd);
	}


	/// <summary>
	/// Settings for the animation curve. Then position the selectable icon and switch it on.
	/// </summary>
	protected override void Init (){
		moveCurve.preWrapMode = WrapMode.Loop;
		moveCurve.postWrapMode = WrapMode.Loop;
		selectableIcon.localPosition = startPos;
		selectableIcon.gameObject.SetActive(true);
	}


	/// <summary>
	/// Each frame, slide the selectable icon between its starting and ending position.
	/// </summary>
	public override void Tick (){
		timer += Time.deltaTime;

		selectableIcon.localPosition = Vector3.Lerp(startPos, endPos, moveCurve.Evaluate(timer));
	}


	/// <summary>
	/// No matter how the task ends, unregister for events and switch off the selectable icon.
	/// </summary>
	protected override void Cleanup (){
		Services.Events.Unregister<NotSelectableEvent>(ListenForDefenderDone);
		Services.Events.Unregister<PhaseStartEvent>(ListenForCombatEnd);
		selectableIcon.gameObject.SetActive(false);
	}


	/// <summary>
	/// When a defender is no longer selectable for any reason, it sends out a NotSelectableEvent.
	/// This script listens for those events, and stops the icon's movement when it receives such
	/// an event for its defender.
	/// </summary>
	/// <param name="e">A NotSelectableEvent.</param>
	private void ListenForDefenderDone(global::Event e){
		Debug.Assert(e.GetType() == typeof(NotSelectableEvent), "Non-NotSelectableEvent in ListenForDefenderDone");

		NotSelectableEvent notEvent = e as NotSelectableEvent;

		if (notEvent.defender == defender) SetStatus(TaskStatus.Success);
	}


	/// <summary>
	/// Stop the icon's movement when the fight phase ends (implicitly occurs if the Besiege Walls phase has begun).
	/// </summary>
	/// <param name="e">A PhaseStartEvent.</param>
	private void ListenForCombatEnd(global::Event e){
		Debug.Assert(e.GetType() == typeof(PhaseStartEvent), "Non-PhaseStartEvent in ListenForCombatEnd");

		PhaseStartEvent phaseEvent = e as PhaseStartEvent;

		if (phaseEvent.Phase.GetType() == typeof(TurnManager.BesiegeWalls)){
			SetStatus(TaskStatus.Success);
		}
	}
}
