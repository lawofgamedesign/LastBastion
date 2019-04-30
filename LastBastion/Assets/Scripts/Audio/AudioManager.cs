using UnityEngine;

public class AudioManager {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//audio sources
	private FadingAudioSource backgroundMusic;
	private FadingAudioSource foley;
	private const string AUDIO_ORGANIZER = "Speakers";
	private const string MUSIC_SPEAKER = "Music";
	private const string FOLEY_SPEAKER = "Foley";

	//clips
	private AudioClip titleMusic; //the music for the title screen
	private AudioClip kitchenMusic;
	private AudioClip tavernMusic;
	private AudioClip battlefieldMusic;
	private AudioClip pauseMusic;
	private AudioClip errorClip; //a default clip which should never play; if this plays, something is wrong!
	private AudioClip kitchenBackgroundSFX;
	private AudioClip tavernBackgroundSFX;
	private AudioClip battleBackgroundSFX;
	private const string MUSIC_PATH = "Audio/Music/";
	private const string SFX_PATH = "Audio/SFX/";


	//the music clip currently playing; useful for, e.g., unpausing the title scene
	private AudioClip currentMusic;

	public enum Clips {
		Error,
		Komiku_Barque_sur_le_Lac,
		Komiku_La_Ville_Aux_Ponts_Suspendus,
		Doctor_Turtle_It_Looks_Like_the_Future_but_Feels_Like_the_Past,
		Doctor_Turtle_Jolenta_Clears_the_Table,
		Komiku_Pirates_Libertaires,
		Komiku_Chill_Out_Theme,
		Kitchen_SFX,
		Tavern_SFX,
		Battlefield_SFX
	}


	//volumes
	public const float MAX_VOLUME = 1.0f;
	
	
	//used to turn sound on or off
	public enum OnOrOff { On, Off }


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables, and then start the audio for the appropriate environment 
	public void Setup(EnvironmentManager.Place place){
		errorClip = Resources.Load<AudioClip>(MUSIC_PATH + Clips.Error.ToString());
		titleMusic = Resources.Load<AudioClip>(MUSIC_PATH + Clips.Komiku_Barque_sur_le_Lac.ToString());
		kitchenMusic = Resources.Load<AudioClip>(MUSIC_PATH + Clips.Doctor_Turtle_It_Looks_Like_the_Future_but_Feels_Like_the_Past.ToString());
		tavernMusic = Resources.Load<AudioClip>(MUSIC_PATH + Clips.Komiku_Pirates_Libertaires.ToString());
		pauseMusic = Resources.Load<AudioClip>(MUSIC_PATH + Clips.Komiku_Chill_Out_Theme.ToString());
		backgroundMusic = new FadingAudioSource(GameObject.Find(AUDIO_ORGANIZER).transform.Find(MUSIC_SPEAKER).GetComponent<AudioSource>(), Clips.Error, MAX_VOLUME, true, false);
		kitchenBackgroundSFX = Resources.Load<AudioClip>(SFX_PATH + Clips.Kitchen_SFX.ToString());
		tavernBackgroundSFX = Resources.Load<AudioClip>(SFX_PATH + Clips.Tavern_SFX.ToString());
		battleBackgroundSFX = Resources.Load<AudioClip>(SFX_PATH + Clips.Battlefield_SFX.ToString());
		foley = new FadingAudioSource(GameObject.Find(AUDIO_ORGANIZER).transform.Find(FOLEY_SPEAKER).GetComponent<AudioSource>(), Clips.Error, MAX_VOLUME, true, false);
		PlayPlaceMusic(place);
	}


	public void PlayPlaceMusic(EnvironmentManager.Place place){
		switch (place){
			case EnvironmentManager.Place.Title_Screen:
				backgroundMusic.Fade(titleMusic, MAX_VOLUME, true);
				
				//start the kitchen background noises
				foley.ToggleSound(OnOrOff.On);
				foley.Fade(kitchenBackgroundSFX, MAX_VOLUME, true);
				break;
			case EnvironmentManager.Place.Kitchen:
				backgroundMusic.Fade(kitchenMusic, MAX_VOLUME, true);
				
				//start the kitchen background noises
				foley.ToggleSound(OnOrOff.On);
				foley.Fade(kitchenBackgroundSFX, MAX_VOLUME, true);
				break;
			case EnvironmentManager.Place.Tavern:
				backgroundMusic.Fade(tavernMusic, MAX_VOLUME, true);
				foley.Fade(tavernBackgroundSFX, MAX_VOLUME, true);
				break;
			case EnvironmentManager.Place.Battlefield:
				
				//start the sword-clashing background sound
				foley.Fade(battleBackgroundSFX, MAX_VOLUME, true);
				break;
		}

		currentMusic = backgroundMusic.GetCurrentClip();
	}


	public void PlayPauseMusic(){
		backgroundMusic.Fade(pauseMusic, MAX_VOLUME, true);
	}


	/// <summary>
	/// Play the standard background music for the environment; useful for the tutorial and title screen.
	/// </summary>
	public void ResumeMusic(){
		backgroundMusic.Fade(currentMusic, MAX_VOLUME, true);
	}


	/// <summary>
	/// Switch all audio sources controlled by the audio manager (i.e., all audio sources) on or off.
	/// </summary>
	/// <param name="onOrOff">Whether the sources should be on or off.</param>
	public void ToggleAllSound(OnOrOff onOrOff){
		backgroundMusic.ToggleSound(onOrOff);
	}
	
	
	


	/// <summary>
	/// Tick fading audio sources
	/// </summary>
	public void Tick(){
		backgroundMusic.Tick();
	}
}
