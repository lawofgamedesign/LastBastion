namespace Tutorial
{
	public class TutorialPhaseStartEvent : Event {

		public readonly FSM<TutorialTurnManager>.State Phase;

		//constructor
		public TutorialPhaseStartEvent(FSM<TutorialTurnManager>.State phase) { 
			this.Phase = phase;
		}
	}
}