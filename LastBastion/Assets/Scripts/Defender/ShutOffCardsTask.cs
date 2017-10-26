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
	/// When this task is active, shut the cards off and then declare the task done.
	/// </summary>
	public override void Tick(){
		GameObject.Find(UI_CANVAS).GetComponent<DefenderUIBehavior>().ShutCardsOff();
		SetStatus(TaskStatus.Success);
	}
}
