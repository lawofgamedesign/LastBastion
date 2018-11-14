using UnityEngine;
using UnityEngine.Video;

public class TutorialManager {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the basic tutorial canvas, with associated variables
	private GameObject tutorialCanvas;
	private const string TUTORIAL_CANVAS_OBJ = "Tutorial canvas";
	private const string SCENE_ORGANIZER = "Scene";
	
	
	//the screen on which videos display
	private VideoPlayer tutorialScreen;
	private const string SCREEN_OBJ = "Tutorial display screen";
	
	
	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////

	
	//initialize variables
	public void Setup(){
		tutorialCanvas = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(TUTORIAL_CANVAS_OBJ),
																GameObject.Find(SCENE_ORGANIZER).transform);
		tutorialScreen = GameObject.Find(SCREEN_OBJ).GetComponent<VideoPlayer>();
	}


	/// <summary>
	/// Play a new tutorial video.
	/// </summary>
	/// <param name="newClip">The clip to play.</param>
	public void PlayTutorial(VideoClip newClip){
		Services.Tasks.AddTask(new TutorialVideoTask(newClip, tutorialCanvas, tutorialScreen));
	}
}
