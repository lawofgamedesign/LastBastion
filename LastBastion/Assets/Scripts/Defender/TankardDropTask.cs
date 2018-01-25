using UnityEngine;

public class TankardDropTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the brawler
	private readonly BrawlerBehavior brawler;


	//number of tankards to drop
	private int drops = 0;


	//used to drop the tankards
	private const string BOARD_TAG = "Board";
	private const string TANKARD_1_OBJ = "Tankard 1";
	private const string TANKARD_2_OBJ = "Tankard 2";
	private const string TANKARD_3_OBJ = "Tankard 3";


	//statements in the chat UI
	private const string DROP_DIRECTIONS = "Choose an empty space for your tankard.";
	private const string DRINK_MSG = "The party's starting!";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public TankardDropTask(BrawlerBehavior brawler, int drops){
		this.brawler = brawler;
		this.drops = drops;
	}


	protected override void Init (){
		Services.Events.Register<InputEvent>(DropTankard);
		Services.Board.HighlightAllEmpty(BoardBehavior.OnOrOff.On);
		Services.UI.OpponentStatement(DROP_DIRECTIONS);
	}


	protected override void OnSuccess (){
		Services.Board.HighlightAllEmpty(BoardBehavior.OnOrOff.Off);
		Services.UI.OpponentStatement(DRINK_MSG);
	}


	protected override void Cleanup (){
		Services.Events.Unregister<InputEvent>(DropTankard);
	}


	/// <summary>
	/// Drop a tankard in a chosen empty space. Also set the space's state accordingly, and provide feedback.
	/// </summary>
	/// <param name="e">An InputEvent with the relevant space.</param>
	private void DropTankard(global::Event e){
		Debug.Assert(e.GetType() == typeof(InputEvent), "Non-InputEvent in DropTankard.");

		InputEvent inputEvent = e as InputEvent;

		if (inputEvent.selected.tag == BOARD_TAG &&
			Services.UI.GetCharSheetStatus() == CharacterSheetBehavior.SheetStatus.Hidden){
			SpaceBehavior space = inputEvent.selected.GetComponent<SpaceBehavior>();

			if (Services.Board.GeneralSpaceQuery(space.GridLocation.x, space.GridLocation.z) != SpaceBehavior.ContentType.None) return;

			Services.Tasks.AddTask(new BlockSpaceFeedbackTask(space.GridLocation.x, space.GridLocation.z, GetTankard()));
			space.Tankard = true;
			GameObject.Find(GetTankard()).GetComponent<TankardBehavior>().GridLoc = new TwoDLoc(space.GridLocation.x, space.GridLocation.z);

			drops--;
		}

		if (drops <= 0) SetStatus(TaskStatus.Success);
	}


	/// <summary>
	/// Provide the appropriate tankard for DropTankard to drop.
	/// </summary>
	/// <returns>The tankard's name, as a string.</returns>
	private string GetTankard(){
		if (brawler.ReportCurrentDrink() == BrawlerBehavior.DrinkTrack.Party_Foul) return TANKARD_1_OBJ;
		else if (drops == 2) return TANKARD_2_OBJ;
		else return TANKARD_3_OBJ;
	}
}
