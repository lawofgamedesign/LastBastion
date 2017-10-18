/// <summary>
/// This event fires every time a board space is clicked.
/// </summary>
public class BoardClickedEvent : Event {

	public readonly TwoDLoc coords;

	public BoardClickedEvent(TwoDLoc coords){
		this.coords = coords;
	}
}
