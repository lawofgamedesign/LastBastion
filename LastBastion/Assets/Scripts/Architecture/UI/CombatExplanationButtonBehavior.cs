using UnityEngine;

public class CombatExplanationButtonBehavior : MonoBehaviour {

	public void DismissExplanation(){
		Services.Events.Fire(new CombatExplanationDoneEvent());
	}
}
