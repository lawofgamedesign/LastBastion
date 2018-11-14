using UnityEngine;

public class TutorialButtonBehavior : MonoBehaviour {

	
	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////
	
	
	public void RequestTutorial(){
		if (Services.Rulebook.TurnMachine.CurrentState.GetType() == typeof(TurnManager.PlayerMove)){
			Services.Tutorials.PlayTutorial(TutorialManager.Tutorials.Move);
		} else if (Services.Rulebook.TurnMachine.CurrentState.GetType() == typeof(TurnManager.PlayerFight)){
			Services.Tutorials.PlayTutorial(TutorialManager.Tutorials.Fight);
		} else {
			Debug.Log("Trying to play a tutorial from a phase that doesn't have one. Current phase: " 
			          + Services.Rulebook.TurnMachine.GetType().ToString());
		}
	}
}
