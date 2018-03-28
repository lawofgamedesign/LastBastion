using UnityEngine;

public class UpgradeTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the defender who's going to upgrade
	private readonly DefenderSandbox defender;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public UpgradeTask(DefenderSandbox defender){
		this.defender = defender;
	}


	/// <summary>
	/// Register for the correct type of event, based on whether this is the tutorial or the full game.
	/// </summary>
	protected override void Init (){
		if (Services.Rulebook.GetType() == typeof(Tutorial.TutorialTurnManager)) Services.Events.Register<TutorialPowerChoiceEvent>(HandlePowerChoices);
		else Services.Events.Register<PowerChoiceEvent>(HandlePowerChoices);

		Services.Defenders.SelectDefenderForUpgrade(defender);
	}


	/// <summary>
	/// If this is the first UpgradeTask, the character sheet will be on the table; pick it up.
	/// 
	/// If this is the second or later UpgradeTask, the character sheet will be on its way back to the table when this
	/// task begins. Wait for it to get there, then take it over and pick it back up.
	/// </summary>
	public override void Tick (){
		if (Services.UI.GetCharSheetStatus() == CharacterSheetBehavior.SheetStatus.Hidden){
			defender.TakeOverCharSheet();
			Services.UI.ShowOrHideSheet();
		}
	}


	/// <summary>
	/// After choosing an upgrade, put the character sheet down.
	/// </summary>
	protected override void OnSuccess (){
		Services.UI.ShowOrHideSheet();
	}


	/// <summary>
	/// No matter how the task finishes, unregister for PowerChoiceEvents.
	/// </summary>
	protected override void Cleanup (){
		if (Services.Rulebook.GetType() == typeof(Tutorial.TutorialTurnManager)) Services.Events.Unregister<TutorialPowerChoiceEvent>(HandlePowerChoices);
		else Services.Events.Unregister<PowerChoiceEvent>(HandlePowerChoices);
	}


	/// <summary>
	/// When the player chooses a new ability, direct that choice appropriately.
	/// </summary>
	/// <param name="e">A PowerChoiceEvent (or TutorialPowerChoiceEvent, if it's the tutorial).</param>
	private void HandlePowerChoices(Event e){
		if (Services.Rulebook.GetType() == typeof(Tutorial.TutorialTurnManager)){
			Debug.Assert(e.GetType() == typeof(TutorialPowerChoiceEvent), "Non-TutorialPowerChoiceEvent in HandlePowerChoices.");

			TutorialPowerChoiceEvent powerEvent = e as TutorialPowerChoiceEvent;

			powerEvent.defender.PowerUp(powerEvent.tree);
		} else {
			Debug.Assert(e.GetType() == typeof(PowerChoiceEvent), "Non-PowerChoiceEvent in HandlePowerChoices.");

			PowerChoiceEvent powerEvent = e as PowerChoiceEvent;

			powerEvent.defender.PowerUp(powerEvent.tree);
		}



		SetStatus(TaskStatus.Success);
	}
}
