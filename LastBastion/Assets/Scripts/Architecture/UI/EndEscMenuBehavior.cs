using UnityEngine;

public class EndEscMenuBehavior : EscMenuBehavior {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the specific menu loaded for the end of the game
	private const string END_MENU_OBJ = "Menus/End menu";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	public override void Setup (){
		menu = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(END_MENU_OBJ));

		base.Setup ();
	}
}
