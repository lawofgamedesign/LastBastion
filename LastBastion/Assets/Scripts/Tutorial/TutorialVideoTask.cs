using UnityEngine;
using UnityEngine.Video;

public class TutorialVideoTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the tutorial video this task will show
	private readonly VideoClip tutorialClip;
	
	
	//the basic tutorial canvas, with associated variables
	private readonly GameObject tutorialCanvas;
	
	
	//the screen on which videos display
	private readonly VideoPlayer tutorialScreen;
	
	
	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public TutorialVideoTask(VideoClip tutorialClip, GameObject tutorialCanvas, VideoPlayer tutorialScreen){
		this.tutorialClip = tutorialClip;
		this.tutorialCanvas = tutorialCanvas;
		this.tutorialScreen = tutorialScreen;
	}


	/// <summary>
	/// Switch on the canvas with the object where videos display, and start playing the video.
	/// </summary>
	protected override void Init(){
		tutorialCanvas.SetActive(true);
		tutorialScreen.clip = tutorialClip;
		tutorialScreen.Play();
	}


	//When the tutorial is done, shut down the tutorial system.
	public override void Tick(){
		if (!tutorialScreen.isPlaying) SetStatus(TaskStatus.Success);
	}


	/// <summary>
	/// When the tutorial is done, shut the canvas containing the video display off.
	/// </summary>
	protected override void OnSuccess(){
		tutorialCanvas.SetActive(false);
	}
}
