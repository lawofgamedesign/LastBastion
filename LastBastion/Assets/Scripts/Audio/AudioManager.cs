using UnityEngine;

public class AudioManager {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//audio sources
	private FadingAudioSource backgroundMusic;
	private const string AUDIO_ORGANIZER = "Speakers";
	private const string MUSIC_SPEAKER = "Music";

	//clips
	private AudioClip kitchenMusic;
	private AudioClip tavernMusic;
	private AudioClip battlefieldMusic;
	private AudioClip pauseMusic;
	private AudioClip sceneStartMusic;
	private const string MUSIC_PATH = "Audio/Music/";

	public enum Clips {
		Komiku_Barque_sur_le_Lac,
		Komiku_La_Ville_Aux_Ponts_Suspendus,
		Doctor_Turtle_It_Looks_Like_the_Future_but_Feels_Like_the_Past,
		Doctor_Turtle_Jolenta_Clears_the_Table,
		Komiku_Pirates_Libertaires,
		Komiku_Chill_Out_Theme
	}


	//volumes
	public const float MAX_VOLUME = 1.0f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables, and then play calm music
	public void Setup(Clips sceneBackground){
		kitchenMusic = Resources.Load<AudioClip>(MUSIC_PATH + Clips.Doctor_Turtle_It_Looks_Like_the_Future_but_Feels_Like_the_Past.ToString());
		tavernMusic = Resources.Load<AudioClip>(MUSIC_PATH + Clips.Komiku_Pirates_Libertaires.ToString());
		pauseMusic = Resources.Load<AudioClip>(MUSIC_PATH + Clips.Komiku_Chill_Out_Theme.ToString());
		sceneStartMusic = Resources.Load<AudioClip>(MUSIC_PATH + sceneBackground.ToString());
		backgroundMusic = new FadingAudioSource(GameObject.Find(AUDIO_ORGANIZER).transform.Find(MUSIC_SPEAKER).GetComponent<AudioSource>(), sceneBackground, MAX_VOLUME, true, true);
	}


	public void PlayPlaceMusic(EnvironmentManager.Place place){
		switch (place){
			case EnvironmentManager.Place.Kitchen:
				backgroundMusic.Fade(kitchenMusic, MAX_VOLUME, true);
				break;
			case EnvironmentManager.Place.Tavern:
				backgroundMusic.Fade(tavernMusic, MAX_VOLUME, true);
				break;
		}
	}


	public void PlayPauseMusic(){
		backgroundMusic.Fade(pauseMusic, MAX_VOLUME, true);
	}


	/// <summary>
	/// Play whatever was the first music played this scene; useful for the tutorial and title screen.
	/// </summary>
	public void PlaySceneStartMusic(){
		backgroundMusic.Fade(sceneStartMusic, MAX_VOLUME, true);
	}


	/// <summary>
	/// Tick fading audio sources
	/// </summary>
	public void Tick(){
		backgroundMusic.Tick();
	}
}
