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
		Services.Events.Register<TutorialStopEvent>(ShutOffTutorial);
	}


	//When the tutorial is done, shut down the tutorial system.
	public override void Tick(){
		if (!tutorialScreen.isPlaying) SetStatus(TaskStatus.Success);
	}

	
	/// <summary>
	/// Pick up TutorialStopEvents fired when the player hits the tutorial stop button.
	/// </summary>
	private void ShutOffTutorial(global::Event e){
		SetStatus(TaskStatus.Aborted);
	}


	/// <summary>
	/// When the tutorial is done, shut the canvas containing the video display off and restore normal sound.
	/// </summary>
	protected override void OnSuccess(){
		Services.Sound.ToggleAllSound(AudioManager.OnOrOff.On);
		tutorialCanvas.SetActive(false);
	}


	/// <summary>
	/// If the player bails out of the tutorial by pressing the stop button, shut the canvas off and restore normal sound.
	/// </summary>
	protected override void OnAbort(){
		Services.Sound.ToggleAllSound(AudioManager.OnOrOff.On);
		tutorialCanvas.SetActive(false);
	}

	
	/// <summary>
	/// No matter how the tutorial ended, be sure to unregister for events.
	/// </summary>
	protected override void Cleanup(){
		Services.Events.Unregister<TutorialStopEvent>(ShutOffTutorial);
	}
}
