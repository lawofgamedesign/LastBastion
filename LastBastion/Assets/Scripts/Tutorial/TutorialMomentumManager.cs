namespace Tutorial
{
	using UnityEngine;

	public class TutorialMomentumManager : MomentumManager {

		/// <summary>
		/// In the tutorial, it's OK to always reset momentum--the attacker doesn't get a turn 2 to use it.
		/// </summary>
		/// <param name="e">An EndPhaseEvent.</param>
		public override void ResetMomentum(global::Event e){
			Debug.Assert(e.GetType() == typeof(EndPhaseEvent), "Non-EndPhaseEvent in ResetMomentum");


			Momentum = START_MOMENTUM;
		}
	}
}
