namespace Title
{
	using UnityEngine;

	public class DetectAnyInput : InputManager {

		/// <summary>
		/// Like InputManager, but fires events whenever an input is detected, regardless of whether the player clicked on anything.
		/// 
		/// The event has a generic gameobject--don't try to use the event's selected gameobject for anything!
		/// </summary>
		public override void Tick(){
			if (Input.GetMouseButtonDown(0)){
				Services.Events.Fire(new InputEvent(new GameObject()));
			}
		}
	}
}
