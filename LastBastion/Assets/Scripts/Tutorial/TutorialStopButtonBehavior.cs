using UnityEngine;

public class TutorialStopButtonBehavior : MonoBehaviour {

	
	/// <summary>
	/// When the player wants to stop the tutorial, send out an event the current TutorialVideoTask can pick up.
	/// </summary>
	public void StopTutorial(){
		Services.Events.Fire(new TutorialStopEvent());
	}
}
