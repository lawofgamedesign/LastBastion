using UnityEngine;

public class PutDownBlockTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the Ranger
	private readonly RangerBehavior ranger;


	//the Ranger's location
	private readonly int rangerX;
	private readonly int rangerZ;


	//the board, so that the task can tell when it's been clicked on
	private const string BOARD_TAG = "Board";


	//the marker for the rockfall's location
	private const string BLOCK_MARKER_OBJ = "Space blocked marker";


	//UI for putting down the rockfall
	private const string ROCK_MSG = "Choose an adjacent, empty space to block.";
	private const string BLOCKED_MSG = "Space blocked!";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public PutDownBlockTask(RangerBehavior ranger){
		this.ranger = ranger;
		rangerX = ranger.ReportGridLoc().x;
		rangerZ = ranger.ReportGridLoc().z;
	}


	protected override void Init (){
		Services.Events.Register<InputEvent>(PutDownBlock);
		Services.UI.OpponentStatement(ROCK_MSG);
		Services.Board.HighlightAllAroundSpace(rangerX, rangerZ, BoardBehavior.OnOrOff.On);
	}


	private void PutDownBlock(Event e){
		Debug.Assert(e.GetType() == typeof(InputEvent), "Non-InputEvent in PutDownBlock");

		//don't put down a block while the player is trying to hide the character sheet
		if (Services.UI.GetCharSheetStatus() == CharacterSheetBehavior.SheetStatus.Displayed) return;

		InputEvent inputEvent = e as InputEvent;

		if (inputEvent.selected.tag == BOARD_TAG){
			SpaceBehavior space = inputEvent.selected.GetComponent<SpaceBehavior>();


			//is this a block that can destroy an attacker?
			bool destroyingBlock = false;

			//try to put down the block
			if (CheckBlockable(space.GridLocation.x, space.GridLocation.z, destroyingBlock)){
				space.Block = true;
				Services.Tasks.AddTask(new BlockSpaceFeedbackTask(space.GridLocation.x, space.GridLocation.z, BLOCK_MARKER_OBJ));
				Services.UI.OpponentStatement(BLOCKED_MSG);

				if (ranger.GetCurrentTrapTrack() == RangerBehavior.TrapTrack.Rockfall) 
					ranger.RockfallLoc = new TwoDLoc(space.GridLocation.x, space.GridLocation.z);

				Services.Events.Unregister<InputEvent>(PutDownBlock);
				Services.Board.HighlightAllAroundSpace(rangerX, rangerZ, BoardBehavior.OnOrOff.Off, true);
				SetStatus(TaskStatus.Success);
			}
		}
	}


	/// <summary>
	/// Is this space one where the Ranger can put the rockfall?
	/// </summary>
	/// <returns><c>true</c> if the space can have the rockfall, <c>false</c> otherwise.</returns>
	/// <param name="x">The x grid coordinate of the space.</param>
	/// <param name="z">The z grid coordinate of the space.</param>
	/// <param name="destroy">Can the rockfall destroy an attacker in the space?</param>
	private bool CheckBlockable(int x, int z, bool destroy){

		//is the space adjacent?
		if (!(Mathf.Abs(x - rangerX) <= 1) ||
			!(Mathf.Abs(z - rangerZ) <= 1)) return false;

		//if this block can't destroy an attacker, return false if the space isn't empty
		if (!destroy){
			if (Services.Board.GeneralSpaceQuery(x, z) != SpaceBehavior.ContentType.None) return false;
		} 
		//if this block can destroy an attacker, return true if the space is empty or has an attacker in it
		else {
			if (Services.Board.GeneralSpaceQuery(x, z) == SpaceBehavior.ContentType.None ||
				Services.Board.GeneralSpaceQuery(x, z) == SpaceBehavior.ContentType.Attacker) return true;
		}

		//if the inquiry gets this far, the space can be blocked
		return true;
	}


	/// <summary>
	/// When the player has put down the rockfall, unregister for inputs.
	/// </summary>
	protected override void Cleanup (){
		Services.Events.Unregister<InputEvent>(PutDownBlock);
	}
}
