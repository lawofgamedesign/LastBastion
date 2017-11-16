using UnityEngine;

public class ReferenceButtonBehavior : MonoBehaviour {


	/// <summary>
	/// When the player clicks the button to display the reference sheet, start displaying it unless the sheet is currently in motion
	/// </summary>
	public void DisplayReferenceSheet(){
		if (!Services.Tasks.CheckForTaskOfType<MoveReferenceSheetTask>()) Services.Tasks.AddTask(new MoveReferenceSheetTask(MoveReferenceSheetTask.Move.Pick_up));
	}


	/// <summary>
	/// Put the reference sheet away.
	/// </summary>
	public void HideReferenceSheet(){
		if (!Services.Tasks.CheckForTaskOfType<MoveReferenceSheetTask>()) Services.Tasks.AddTask(new MoveReferenceSheetTask(MoveReferenceSheetTask.Move.Put_down));
	}
}
