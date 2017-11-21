using UnityEngine;

public class UndoButtonBehavior : MonoBehaviour {

	public void UndoPhase(){
		if (Services.Undo == null) return; //if there's somehow no undo system, don't do anything

		Services.Undo.UndoMovePhase();
	}
}
