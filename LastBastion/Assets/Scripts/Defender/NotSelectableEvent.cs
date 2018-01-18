public class NotSelectableEvent : Event {

	public readonly DefenderSandbox defender;


	public NotSelectableEvent(DefenderSandbox defender){
		this.defender = defender;
	}
}
