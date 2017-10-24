using UnityEngine;

public class InputManager {


	public void Tick(){
		if (Input.GetMouseButton(0)){
			GameObject selected = GetClickedThing();

			if (selected != null) Services.Events.Fire(new InputEvent(selected));
		}
	}


	/// <summary>
	/// Use a raycast to find what, if anything, was clicked on.
	/// 
	/// Note that this returns null if nothing was selected; it's up to the calling function to check for null returns.
	/// </summary>
	/// <returns>The clicked thing.</returns>
	private GameObject GetClickedThing(){
		RaycastHit hit;
		GameObject obj = null;

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out hit)) obj = hit.collider.gameObject;

		return obj;
	}
}
