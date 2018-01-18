using UnityEngine;

public class ChangeEnvironmentTask : Task {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the environment organizer
	private readonly Transform environment;
	private const string ENVIRONMENT_ORGANIZER = "Environment";


	//which environment are we changing to?
	private readonly EnvironmentManager.Place newPlace;


	//which environment are we changing from?
	private readonly EnvironmentManager.Place oldPlace;


	//other settings to change
	private readonly Color ambientColor; //the color of the ambient light


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public ChangeEnvironmentTask(EnvironmentManager.Place newPlace, EnvironmentManager.Place oldPlace, Color ambientColor){
		environment = GameObject.Find(ENVIRONMENT_ORGANIZER).transform;

		this.newPlace = newPlace;
		this.oldPlace = oldPlace;
		this.ambientColor = ambientColor;
	}


	/// <summary>
	/// Activate the new environment and deactivate the old one. Change other settings as necessary
	/// </summary>
	public override void Tick (){
		environment.Find(newPlace.ToString()).gameObject.SetActive(true);
		environment.Find(oldPlace.ToString()).gameObject.SetActive(false);
		RenderSettings.ambientLight = ambientColor;
		SetStatus(Task.TaskStatus.Success);
	}
}
