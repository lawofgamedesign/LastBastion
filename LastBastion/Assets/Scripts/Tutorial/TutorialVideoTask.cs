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
	
	
	//game speeds
	private const float ZERO = 0.0f;
	private const float ONE = 1.0f;
	
	
	//timer for video clips
	//note that VideoPlayers can't be relied upon to start playing immediately upon Play() being called;
	//tracking time avoids this issue
	private float clipDuration = 0.0f;
	private float timer = 0.0f;
	
	
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
		Debug.Assert(tutorialClip != null, "No tutorial clip provided");
		tutorialCanvas.SetActive(true);
		tutorialScreen.clip = tutorialClip;
		tutorialScreen.Play();
		clipDuration = (float)tutorialClip.length;
		Services.Sound.ToggleAllSound(AudioManager.OnOrOff.Off); //shut off all other sound
		if (Services.UI != null) Services.UI.ToggleAllButtons(ChatUI.OnOrOff.Off); //if in-game, switch off all UI buttons
		Time.timeScale = ZERO;
		Services.Events.Register<TutorialStopEvent>(ShutOffTutorial);
	}


	//When the tutorial is done, shut down the tutorial system.
	public override void Tick(){
		timer += Time.unscaledDeltaTime;
		
		if (timer >= clipDuration) SetStatus(TaskStatus.Success);
	}

	
	/// <summary>
	/// Pick up TutorialStopEvents fired when the player hits the tutorial stop button.
	/// </summary>
	private void ShutOffTutorial(global::Event e){
		SetStatus(TaskStatus.Aborted);
	}


	/// <summary>
	/// Do everything needed to end the tutorial process and restore normal gameplay.
	/// </summary>
	private void EndTutorial(){
		Services.Sound.ToggleAllSound(AudioManager.OnOrOff.On);
		tutorialCanvas.SetActive(false);
		if (Services.UI != null) Services.UI.ToggleAllButtons(ChatUI.OnOrOff.On); //switch menu buttons back on
		Services.Events.Fire(new PauseEvent(PauseEvent.Pause.Unpause)); //send out an event the title scene can pick up to start rotating again
		Time.timeScale = ONE;
	}


	/// <summary>
	/// When the tutorial is done, shut the canvas containing the video display off and restore normal sound.
	/// </summary>
	protected override void OnSuccess(){
		EndTutorial();
	}


	/// <summary>
	/// If the player bails out of the tutorial by pressing the stop button, shut the canvas off and restore normal sound.
	/// </summary>
	protected override void OnAbort(){
		EndTutorial();
	}

	
	/// <summary>
	/// No matter how the tutorial ended, be sure to unregister for events.
	/// </summary>
	protected override void Cleanup(){
		Services.Events.Unregister<TutorialStopEvent>(ShutOffTutorial);
	}
}
