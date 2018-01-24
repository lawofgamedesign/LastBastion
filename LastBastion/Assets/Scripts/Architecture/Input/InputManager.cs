using UnityEngine;

public class InputManager {


	//how close to the edge of the screen the input device needs to be to rotate the camera,
	//as a percentage
	private const float SCREEN_MARGIN = 5.0f;


	public virtual void Tick(){
		if (Input.GetMouseButtonDown(0)){
			GameObject selected = GetClickedThing();

			if (selected != null) Services.Events.Fire(new InputEvent(selected));
		}

		if (Input.mousePosition.y/Screen.height >= (100.0f - SCREEN_MARGIN)/100.0f){
			Services.PlayerEyes.CameraUp();
		} else if (Input.mousePosition.x/Screen.width <= SCREEN_MARGIN/100.0f) {
			Services.PlayerEyes.CameraLeft();
		} else if (Input.mousePosition.x/Screen.width >= (100.0f - SCREEN_MARGIN)/100.0f) {
			Services.PlayerEyes.CameraRight();
		} else {
			Services.PlayerEyes.CameraToTable();
		}
	}


	/// <summary>
	/// Use a raycast to find what, if anything, was clicked on.
	/// 
	/// Note that this returns null if nothing was selected; it's up to the calling function to check for null returns.
	/// </summary>
	/// <returns>The clicked thing.</returns>
	protected GameObject GetClickedThing(){
		RaycastHit hit;
		GameObject obj = null;

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if (Physics.Raycast(ray, out hit)) obj = hit.collider.gameObject;

		return obj;
	}
}
