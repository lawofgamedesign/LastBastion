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


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public ChangeEnvironmentTask(EnvironmentManager.Place newPlace, EnvironmentManager.Place oldPlace){
		environment = GameObject.Find(ENVIRONMENT_ORGANIZER).transform;

		this.newPlace = newPlace;
		this.oldPlace = oldPlace;
	}


	public override void Tick (){
		environment.Find(newPlace.ToString()).gameObject.SetActive(true);
		environment.Find(oldPlace.ToString()).gameObject.SetActive(false);
		SetStatus(Task.TaskStatus.Success);
	}
}
