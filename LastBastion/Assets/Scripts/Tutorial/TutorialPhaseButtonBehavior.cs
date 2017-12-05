namespace Tutorial
{
	using UnityEngine;

	public class TutorialPhaseButtonBehavior : MonoBehaviour {

		/// <summary>
		/// Send out an EndPhaseEvent. In the tutorial, there's no need to worry about which phase it is.
		/// </summary>
		public void EndPhase(){
			Services.Events.Fire(new EndPhaseEvent());
		}
	}
}
