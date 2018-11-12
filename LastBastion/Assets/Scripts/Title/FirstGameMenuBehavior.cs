using Title;
using UnityEngine;

public class FirstGameMenuBehavior : MonoBehaviour {

	
	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the title manager, for swapping menus
	private const string MANAGER_OBJ = "Game managers";
	
	
	//the introductory audio
	private AudioClip introClip;
	
	
	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////
	
	
	
	
	
	public void SwitchOff(){
		GameObject.Find(MANAGER_OBJ).GetComponent<TitleManager>().SwitchMenus();
	}
}
