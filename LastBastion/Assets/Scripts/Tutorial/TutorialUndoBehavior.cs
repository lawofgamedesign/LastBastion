namespace Tutorial
{
	using UnityEngine;

	public class TutorialUndoBehavior : UndoButtonBehavior {

		public override void UndoPhase (){
			base.UndoPhase ();

			Services.Events.Fire(new UndoMoveEvent());
		}
	}
}
