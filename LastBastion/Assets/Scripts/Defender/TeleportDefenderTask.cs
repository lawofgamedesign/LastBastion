using System.Collections.Generic;
using UnityEngine;

public class TeleportDefenderTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the defender to move
	private readonly DefenderSandbox defender;


	//places where the defender can teleport to
	public enum PossibleDestinations
	{
		Adjacent,
		Any_open
	};
	private readonly PossibleDestinations destination;
	
	
	//used to determine where the defender should teleport to
	private const string BOARD_TAG = "Board";
	
	
	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public TeleportDefenderTask(DefenderSandbox defender, PossibleDestinations destination){
		this.defender = defender;
		this.destination = destination;
	}


	/// <summary>
	/// Highlight the spaces to which the defender can move, and start listening for InputEvents.
	/// </summary>
	protected override void Init(){
		if (destination == PossibleDestinations.Any_open) Services.Board.HighlightAllEmpty(BoardBehavior.OnOrOff.On);
		else if (destination == PossibleDestinations.Adjacent) 
			Services.Board.HighlightAllAroundSpace(defender.ReportGridLoc().x,
												   defender.ReportGridLoc().z,
												   BoardBehavior.OnOrOff.On,
												   true);
		else Debug.Log("Trying to teleport to an impossible location.");
		
		Services.Events.Register<InputEvent>(SelectDestination);
	}


	/// <summary>
	/// Handle player attempts to choose a space to teleport to:
	///
	/// 1. Confirm that the player chose a space.
	/// 2. Confirm that the space is empty.
	/// 3. If the player is only allowed to move to adjacent spaces, confirm that the space is adjacent.
	/// 4. Start a task that moves the defender to the chosen space.
	/// </summary>
	/// <param name="e"></param>
	private void SelectDestination(global::Event e){
		Debug.Assert(e.GetType() == typeof(InputEvent), "Non-InputEvent in SelectDestination");
		
		InputEvent inputEvent = e as InputEvent;

		if (inputEvent.selected.tag == BOARD_TAG &&
		    Services.UI.GetCharSheetStatus() == CharacterSheetBehavior.SheetStatus.Hidden){
			
			SpaceBehavior space = inputEvent.selected.GetComponent<SpaceBehavior>();

			//stop if the player chose a space that has something in it
			if (Services.Board.GeneralSpaceQuery(space.GridLocation.x, space.GridLocation.z) != SpaceBehavior.ContentType.None) return;
			
			//stop if the defender needs to teleport to an adjacent space and the selected space is not adjacent
			if (destination == PossibleDestinations.Adjacent)
			{
				if (!Services.Board.CheckAdjacentSpace(defender.ReportGridLoc().x,
					defender.ReportGridLoc().z,
					space.GridLocation.x,
					space.GridLocation.z)) return;
			}

			//physically move the defender model
			Services.Tasks.AddTask(new MoveDefenderTask(defender.gameObject.GetComponent<Rigidbody>(), 
								   defender.GetScreenMoveSpeed(),
								   new List<TwoDLoc>() { defender.ReportGridLoc(), space.GridLocation }));
			
			
			//update the board
			Services.Board.TakeThingFromSpace(defender.ReportGridLoc().x, defender.ReportGridLoc().z);
			Services.Board.PutThingInSpace(defender.gameObject,
										   space.GridLocation.x,
										   space.GridLocation.z,
										   SpaceBehavior.ContentType.Defender);
			defender.NewLoc(space.GridLocation.x, space.GridLocation.z);
			
			SetStatus(TaskStatus.Success);
		}
	}


	/// <summary>
	/// No matter how the task ends, unregister for InputEvents and shut off highlights.
	/// </summary>
	protected override void Cleanup(){
		if (destination == PossibleDestinations.Any_open) Services.Board.HighlightAllEmpty(BoardBehavior.OnOrOff.Off);
		else if (destination == PossibleDestinations.Adjacent) 
			Services.Board.HighlightAllAroundSpace(defender.ReportGridLoc().x, defender.ReportGridLoc().z, BoardBehavior.OnOrOff.Off);
		else Debug.Log("Trying to teleport to an impossible location.");
		
		Services.Events.Unregister<InputEvent>(SelectDestination);
	}
}
