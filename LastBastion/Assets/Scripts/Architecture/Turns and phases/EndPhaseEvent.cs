public class EndPhaseEvent : Event {


	public readonly FSM<TurnManager>.State Phase;


	//constructor
	public EndPhaseEvent(FSM<TurnManager>.State phase) { 
		this.Phase = phase;
	}

	//default constructor, for when the phase doesn't matter
	public EndPhaseEvent() { }
}
