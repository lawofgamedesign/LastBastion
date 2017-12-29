using UnityEngine;

public class UndoButtonBehavior : MonoBehaviour {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//message sent to the chat window on an undo
	private const string UNDO_MSG = "Wait, let me start over.";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	public void UndoPhase(){
		if (Services.Undo == null) return; //if there's somehow no undo system, don't do anything

		Services.Tasks.AddTask(new MoveBalloonTask(transform.position, UNDO_MSG, MoveBalloonTask.GrowOrShrink.Shrink));

		Services.Undo.UndoMovePhase();
	}
}
