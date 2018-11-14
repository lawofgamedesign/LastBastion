using UnityEngine;
using UnityEngine.Video;

public class TutorialManager {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the basic tutorial canvas, with associated variables
	private GameObject tutorialCanvas;
	private const string TUTORIAL_CANVAS_OBJ = "Tutorial video canvas";
	private const string SCENE_ORGANIZER = "Scene";
	
	
	//the screen on which videos display
	private VideoPlayer tutorialScreen;
	private const string SCREEN_OBJ = "Tutorial display screen";
	
	
	//tutorial videos
	private VideoClip fullTutorial;
	private VideoClip moveTutorial;
	private VideoClip fightTutorial;
	private const string TUTORIAL_MOV_PATH = "Video/";
	private const string FULL_TUTORIAL_MOV = "Full tutorial clip";
	private const string MOVE_TUTORIAL_MOV = "Move tutorial clip";
	private const string FIGHT_TUTORIAL_MOV = "Fight tutorial clip";
	public enum Tutorials { Full, Move, Fight }


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////

	
	//initialize variables
	public void Setup(){
		tutorialCanvas = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(TUTORIAL_CANVAS_OBJ),
																GameObject.Find(SCENE_ORGANIZER).transform);
		tutorialScreen = GameObject.Find(SCREEN_OBJ).GetComponent<VideoPlayer>();
		tutorialCanvas.SetActive(false);
		
		fullTutorial = Resources.Load<VideoClip>(TUTORIAL_MOV_PATH + FULL_TUTORIAL_MOV);
		moveTutorial = Resources.Load<VideoClip>(TUTORIAL_MOV_PATH + MOVE_TUTORIAL_MOV);
		fightTutorial = Resources.Load<VideoClip>(TUTORIAL_MOV_PATH + FIGHT_TUTORIAL_MOV);
	}


	/// <summary>
	/// Play a new tutorial video.
	/// </summary>
	/// <param name="tutorial">The tutorial to play.</param>
	public void PlayTutorial(Tutorials tutorial){
		Services.Tasks.AddTask(new TutorialVideoTask(ChooseVideo(tutorial), tutorialCanvas, tutorialScreen));
	}


	/// <summary>
	/// Get the correct video for a given tutorial.
	/// </summary>
	/// <param name="tutorial">The type of tutorial needed.</param>
	/// <returns>The video clip for the chosen tutorial.</returns>
	private VideoClip ChooseVideo(Tutorials tutorial){
		if (tutorial == Tutorials.Full) return fullTutorial;
		else if (tutorial == Tutorials.Move) return moveTutorial;
		else if (tutorial == Tutorials.Fight) return fightTutorial;
		else{
			Debug.Log("Trying to display a non-existent tutorial: " + tutorial.ToString());
			return fullTutorial;
		}
	}
}
