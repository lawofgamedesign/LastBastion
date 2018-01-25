using UnityEngine;

public class RockfallTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the ranger, for checking the Ranger's location
	private readonly RangerBehavior ranger;


	//the board and the marker for where the rockfall has landed
	private const string BOARD_TAG = "Board";
	private const string BLOCK_MARKER_OBJ = "Space blocked marker";


	//statements made in the chat UI
	private const string ROCK_MSG = "Choose an adjacent, empty space to block.";
	private const string BLOCKED_MSG = "Space blocked!";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	public RockfallTask(RangerBehavior ranger){
		this.ranger = ranger;
	}


	protected override void Init (){
		Services.Board.HighlightAllAroundSpace(ranger.ReportGridLoc().x, ranger.ReportGridLoc().z, BoardBehavior.OnOrOff.On, true);
		Services.UI.OpponentStatement(ROCK_MSG);
		Services.Events.Register<InputEvent>(PutDownBlock);
	}


	protected override void Cleanup (){
		Services.Events.Unregister<InputEvent>(PutDownBlock);
	}


	private void PutDownBlock(global::Event e){
		Debug.Assert(e.GetType() == typeof(InputEvent), "Non-InputEvent in PutDownBlock");

		InputEvent inputEvent = e as InputEvent;

		if (inputEvent.selected.tag == BOARD_TAG){
			SpaceBehavior space = inputEvent.selected.GetComponent<SpaceBehavior>();

			//if the space isn't blockable for any reason, stop
			if (!CheckBlockable(space.GridLocation)) return;


			//the space is blockable; put down the rockfall and provide appropriate feedback
			space.Block = true;
			Services.Tasks.AddTask(new BlockSpaceFeedbackTask(space.GridLocation.x, space.GridLocation.z, BLOCK_MARKER_OBJ));


			//tell the Ranger where the rockfall is
			if (ranger.GetCurrentTrapTrack() == RangerBehavior.TrapTrack.Rockfall) ranger.RockfallLoc = new TwoDLoc(space.GridLocation.x, space.GridLocation.z);


			Services.Board.HighlightAllAroundSpace(ranger.ReportGridLoc().x, ranger.ReportGridLoc().z, BoardBehavior.OnOrOff.Off, true);

			SetStatus(TaskStatus.Success);
		}
	}


	/// <summary>
	/// Call this to determine whether the rockfall can occur in a given space.
	/// </summary>
	/// <returns><c>true</c> if the space meets all requirements for the rockfall occurring there, <c>false</c> otherwise.</returns>
	/// <param name="spaceLoc">The space's location in the grid.</param>
	private bool CheckBlockable(TwoDLoc spaceLoc){
		//is the space adjacent?
		if (!(Mathf.Abs(spaceLoc.x - ranger.ReportGridLoc().x) <= 1) ||
			!(Mathf.Abs(spaceLoc.z - ranger.ReportGridLoc().z) <= 1)) return false;

		//is the space empty?
		if (Services.Board.GeneralSpaceQuery(spaceLoc.x, spaceLoc.z) != SpaceBehavior.ContentType.None) return false;

		return true;
	}
}
