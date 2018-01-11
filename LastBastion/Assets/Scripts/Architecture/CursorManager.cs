using UnityEngine;

public class CursorManager {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//path to cursor textures
	private const string CURSOR_FOLDER = "Cursors";


	//offset from the upper-left that is the actual pixel used to make selections
	private Vector2 hotspot = new Vector2(0.0f, 0.0f);


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	public void Setup(){
		Texture2D[] cursors = Resources.LoadAll<Texture2D>(CURSOR_FOLDER);

		Cursor.SetCursor(cursors[Random.Range(0, cursors.Length)], hotspot, CursorMode.Auto);
	}
}
