public class ExplanationEvent : Event {


	public readonly ExplainButtonBehavior.CurrentState buttonState;

	public ExplanationEvent(ExplainButtonBehavior.CurrentState buttonState){
		this.buttonState = buttonState;
	}
}
