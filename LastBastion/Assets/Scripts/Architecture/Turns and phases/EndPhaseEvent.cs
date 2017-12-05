public class EndPhaseEvent : Event {


	public readonly FSM<TurnManager>.State Phase;


	public EndPhaseEvent(FSM<TurnManager>.State phase) { 
		this.Phase = phase;
	}
}
