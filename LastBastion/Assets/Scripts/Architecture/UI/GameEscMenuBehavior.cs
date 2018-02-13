using UnityEngine;

public class GameEscMenuBehavior : EscMenuBehavior {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the specific menu loaded for the game scene
	private const string GAME_MENU_OBJ = "Menus/Game screen menu";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	public override void Setup (){
		menu = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(GAME_MENU_OBJ));

		base.Setup ();
	}
}
