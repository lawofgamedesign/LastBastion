using UnityEngine;

public class CursorManager {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the current cursor
	public Sprite CurrentCursor { get; private set; }


	//path to cursor textures
	private const string CURSOR_FOLDER = "Cursors";


	//offset from the upper-left that is the actual pixel used to make selections
	private Vector2 hotspot = new Vector2(0.0f, 0.0f);


	//used to turn the cursor on or off
	public enum OnOrOff { On, Off };


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//set the cursor's graphic. Only the title screen needs to do this.
	public void Setup(){
		Texture2D[] cursors = Resources.LoadAll<Texture2D>(CURSOR_FOLDER);

		Texture2D currentCursorTexture = cursors[Random.Range(0, cursors.Length)];
		CurrentCursor = Sprite.Create(currentCursorTexture, new Rect(0.0f, 0.0f, 31.0f, 32.0f),
									  Vector2.zero);
		
		Cursor.SetCursor(cursors[Random.Range(0, cursors.Length)], hotspot, CursorMode.Auto);
	}


	/// <summary>
	/// Switch the cursor's graphic on or off.
	/// </summary>
	/// <param name="onOrOff">Whether the graphic should appear or not.</param>
	public void ToggleCursor(OnOrOff onOrOff){
		if (onOrOff == OnOrOff.On) Cursor.visible = true;
		else Cursor.visible = false;
	}
}
