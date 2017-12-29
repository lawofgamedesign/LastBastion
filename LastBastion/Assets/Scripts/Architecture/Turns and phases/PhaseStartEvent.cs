public class PhaseStartEvent : Event {

	public readonly FSM<TurnManager>.State Phase;

	//constructor
	public PhaseStartEvent(FSM<TurnManager>.State phase) { 
		this.Phase = phase;
	}
}
