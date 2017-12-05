using UnityEngine;

public class EndPhaseButtonBehavior : MonoBehaviour {

	/// <summary>
	/// Send out an EndPhaseEvent with the phase that is ending.
	/// </summary>
	public void EndPhase(){
		Services.Events.Fire(new EndPhaseEvent(Services.Rulebook.TurnMachine.CurrentState));
	}
}
