using UnityEngine;

public class AudioManager {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//audio sources, and information required to find them
	private AudioSource musicSpeaker;
	private const string MUSIC_OBJ = "Music";


	//clips
	private AudioClip calmMusic;
	private AudioClip dangerMusic;
	private const string MUSIC_PATH = "Audio/Music/";
	private const string CALM_CLIP = "Doctor Turtle - It Looks Like the Future but Feels Like the Past";
	private const string DANGER_CLIP = "Doctor Turtle - Jolenta Clears the Table";


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables, and then play calm music
	public void Setup(){
		musicSpeaker = GameObject.Find(MUSIC_OBJ).GetComponent<AudioSource>();
		calmMusic = Resources.Load<AudioClip>(MUSIC_PATH + CALM_CLIP);
		dangerMusic = Resources.Load<AudioClip>(MUSIC_PATH + DANGER_CLIP);
		PlayCalmMusic();
	}


	public void PlayCalmMusic(){
		if (musicSpeaker.clip == calmMusic) return; //don't do anything if calm music is already playing
		musicSpeaker.clip = calmMusic;
		musicSpeaker.Play();
	}


	public void PlayDangerMusic(){
		if (musicSpeaker.clip == dangerMusic) return; //don't do anything if the danger music is already playing
		musicSpeaker.clip = dangerMusic;
		musicSpeaker.Play();
	}
}
