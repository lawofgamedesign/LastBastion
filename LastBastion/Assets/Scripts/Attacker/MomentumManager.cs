using UnityEngine;

public class MomentumManager {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the attackers' momentum
	public int Momentum { get; protected set; }
	protected const int START_MOMENTUM = 0;


	//UI
	private GameObject marker; //the marker that gets put down
	private const string MARKER_OBJ = "Momentum marker";
	private Vector3 markerCenter = new Vector3(0.0f, 0.0f, 0.0f); //the center point of the area where markers get "tossed";
	private const string MARKER_CANVAS_OBJ = "Momentum canvas";

	//the furthest a marker will deviate from the center of the area
	private Transform markerOrganizer;
	private const string MARKER_ORGANIZER = "Markers and tokens";
	private const float MAX_VERT_OFFSET = 1.5f;
	private const float MAX_HORIZ_OFFSET = 3.0f;
	private const float MAX_ROTATION = 359.9f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public void Setup(){
		Momentum = START_MOMENTUM;
		Services.Events.Register<MissedFightEvent>(IncreaseMomentum);
		Services.Events.Register<EndPhaseEvent>(ResetMomentum);
		marker = Resources.Load<GameObject>(MARKER_OBJ);
		markerCenter = GameObject.Find(MARKER_CANVAS_OBJ).transform.position;
		markerOrganizer = GameObject.Find(MARKER_ORGANIZER).transform;
	}


	public void IncreaseMomentum(Event e){
		Debug.Assert(e.GetType() == typeof(MissedFightEvent), "Non-MissedFightEvent in IncreaseMomentum.");

		Momentum++;

		AddMarker();
	}


	public virtual void ResetMomentum(Event e){
		Debug.Assert(e.GetType() == typeof(EndPhaseEvent), "Non-EndPhaseEvent in ResetMomentum");

		EndPhaseEvent endEvent = e as EndPhaseEvent;

		if (endEvent.Phase.GetType() == typeof(TurnManager.AttackersAdvance)){
			Momentum = START_MOMENTUM;

			GameObject[] momentumMarkers = GameObject.FindGameObjectsWithTag(MARKER_OBJ);

			foreach (GameObject marker in momentumMarkers){

				//use tasks written for attackers; they work fine for this purpose
				EjectAttackerTask ejectTask = new EjectAttackerTask(marker.GetComponent<Rigidbody>());
				ejectTask.Then(new DestroyAttackerTask(marker));
				Services.Tasks.AddTask(ejectTask);
			}
		}
	}


	/// <summary>
	/// Put down a marker to show how much momentum the attackers have.
	/// </summary>
	private void AddMarker(){
		Vector3 pos = markerCenter;
		pos.x += Random.Range(-MAX_HORIZ_OFFSET, MAX_HORIZ_OFFSET);
		pos.z += Random.Range(-MAX_VERT_OFFSET, MAX_VERT_OFFSET);

		Vector3 rot = new Vector3(0.0f, Random.Range(0.0f, MAX_ROTATION), 0.0f);

		MonoBehaviour.Instantiate(marker, pos, Quaternion.Euler(rot), markerOrganizer);
	}
}
