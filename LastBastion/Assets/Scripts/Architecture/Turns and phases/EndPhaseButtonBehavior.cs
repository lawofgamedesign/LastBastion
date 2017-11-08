using UnityEngine;

public class EndPhaseButtonBehavior : MonoBehaviour {

	public void EndPhase(){
		Services.Events.Fire(new EndPhaseEvent());
	}
}
