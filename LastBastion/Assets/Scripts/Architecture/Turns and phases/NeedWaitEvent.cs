public class NeedWaitEvent : Event
{
	public readonly bool needWait;


	/// <summary>
	/// Constructor for an event signaling TurnManager that there is a need to go to the special
	/// Waiting state.
	/// </summary>
	public NeedWaitEvent(bool needWait){
		this.needWait = needWait;
	}
}
