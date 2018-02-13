using UnityEngine;

public class TitleEscMenuBehavior : EscMenuBehavior {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the specific menu loaded for the title scene
	private const string TITLE_MENU_OBJ = "Menus/Title screen menu";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	public override void Setup (){
		menu = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(TITLE_MENU_OBJ));
			
		base.Setup ();
	}
}
