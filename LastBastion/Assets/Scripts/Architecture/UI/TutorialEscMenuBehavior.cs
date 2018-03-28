using UnityEngine;

public class TutorialEscMenuBehavior : EscMenuBehavior {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the specific menu loaded for the tutorial scene
	private const string TUTORIAL_MENU_OBJ = "Menus/Tutorial screen menu";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	public override void Setup (){
		menu = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(TUTORIAL_MENU_OBJ));

		base.Setup ();
	}
}
