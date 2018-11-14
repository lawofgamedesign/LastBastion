using UnityEngine;

public class FadingAudioSource {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////



	//volume at which fading out clips stop playing
	public float fadeOutThreshold = 0.05f;


	///volume change/second
	public float fadeSpeed = 0.5f;


	//the audio source
	private AudioSource source;


	//used to fade in and out
	public enum FadeState { None, Out, In }
	private FadeState fadeState = FadeState.None;


	//clip to fade to
	private AudioClip nextClip;


	//whether to loop the next clip
	private bool loopNextClip;


	//fade the next clip in until it reaches this volume
	private float nextClipVolume;


	//path to load clips
	private const string MUSIC_PATH = "Audio/Music/";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public FadingAudioSource(AudioSource source, AudioManager.Clips startClip, float volume, bool loop, bool startPlaying){
		this.source = source;
		source.clip = Resources.Load<AudioClip>(MUSIC_PATH + startClip.ToString());
		source.volume = volume;
		source.loop = loop;
		if (startPlaying) source.Play();
	}


	/// <summary>
	/// Fade from the current clip to a new one.
	/// </summary>
	/// <param name="clip">The clip to fade to.</param>
	/// <param name="volume">The new clip's volume.</param>
	/// <param name="loop">If set to <c>true</c>, loop the new clip.</param>
	public void Fade(AudioClip clip, float volume, bool loop){

		//do nothing if there's an attempt to fade to nothing, or to fade to the same clip again
		if (clip == null || clip == source.clip) {
			Debug.Log("Unable to fade; clip == " + clip);
			return;
		}


		nextClip = clip;
		nextClipVolume = volume;
		loopNextClip = loop;

		if (source.enabled){
			if (source.isPlaying) fadeState = FadeState.Out;
			else FadeToNextClip();
		} else {
			FadeToNextClip();
		}
	}


	/// <summary>
	/// Go to the next clip;
	/// </summary>
	private void FadeToNextClip(){
		source.clip = nextClip;
		source.loop = loopNextClip;


		fadeState = FadeState.In;

		if (source.enabled) source.Play();
	}


	/// <summary>
	/// Switch the sound for this source on or off.
	/// </summary>
	/// <param name="onOrOff">Whether the sound should be on or off.</param>
	public void ToggleSound(AudioManager.OnOrOff onOrOff){
		if (onOrOff == AudioManager.OnOrOff.On) source.enabled = true;
		else source.enabled = false;
	}


	/// <summary>
	/// Each frame, fade in or out if needed.
	/// </summary>
	public void Tick(){

		//if the audio source is not enabled or no fading is occurring, do nothing
		if (!source.enabled || fadeState == FadeState.None) return;


		//if fading out, reduce volume until below the threshold, then start fading in
		if (fadeState == FadeState.Out){
			if (source.volume  > fadeOutThreshold){
				source.volume -= fadeSpeed * Time.deltaTime;
			} else {
				FadeToNextClip();
			}

		//if fading in, increase volume until reaching the intended volume
		} else if (fadeState == FadeState.In){
			if (source.volume < nextClipVolume){
				source.volume += fadeSpeed * Time.deltaTime;
			} else {
				fadeState = FadeState.None;
			}
		}
	}
}
