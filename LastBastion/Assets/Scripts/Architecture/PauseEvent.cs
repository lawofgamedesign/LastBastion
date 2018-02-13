public class PauseEvent : Event {

	public enum Pause { Pause, Unpause };
	public readonly Pause action;


	public PauseEvent(Pause action){
		this.action = action;
	}
}
