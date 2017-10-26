/*
 * 
 * Simple task for queueing shutting off defender combat cards.
 * 
 */ 
using UnityEngine;

public class ShutOffCardsTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	private const string UI_CANVAS = "Defender card canvas";


	/////////////////////////////////////////////
	/// Function
	/////////////////////////////////////////////


	//constructor
	public ShutOffCardsTask(){
		//intentionally blank
	}


	/// <summary>
	/// When this task is active, shut the cards off and then declare the task done so long as no one is selected.
	/// 
	/// If any defenders are currently selected, don't shut the cards off--we assume someone is using them!
	/// </summary>
	public override void Tick(){
		if (!Services.Defenders.IsAnyoneSelected()){
			GameObject.Find(UI_CANVAS).GetComponent<DefenderUIBehavior>().ShutCardsOff();
			SetStatus(TaskStatus.Success);
		} else {
			SetStatus(TaskStatus.Aborted);
		}
	}
}
