using Title;
using UnityEngine;
using UnityEngine.Video;

public class FirstGameMenuBehavior : MonoBehaviour {

	
	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the title manager, for swapping menus
	private const string MANAGER_OBJ = "Game managers";
	
	
	//the introductory video, with associated variables
	private VideoClip introClip;
	
	
	
	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	/// <summary>
	/// Play the intro video.
	/// </summary>
	public void PlayIntro(){
		Services.Sound.ToggleAllSound(AudioManager.OnOrOff.Off);
		Services.Events.Fire(new PauseEvent(PauseEvent.Pause.Pause));
		Services.Tutorials.PlayTutorial(TutorialManager.Tutorials.Intro);
	}
	
	
	/// <summary>
	/// Shut the first game menu off, and show the normal title menu.
	/// </summary>
	public void SwitchOff(){
		GameObject.Find(MANAGER_OBJ).GetComponent<TitleManager>().SwitchMenus();
	}
}
