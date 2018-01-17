using UnityEngine;

public class ChangeFogTask : Task {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//potential fog values
	private const string START_FOG_HEX = "808080FF";
	private const float LOW_FOG_DENSITY = 0.005f;
	private const float HIGH_FOG_DENSITY = 0.05f;


	//time to ending value, in seconds
	private float changeTime = 1.0f;


	//which change is happening?
	public enum DenseOrLight { Dense, Light }


	//values this task will use
	private readonly float startDensity = 0.0f;
	private readonly float endDensity = 0.0f;
	private readonly DenseOrLight change;
	private float timer = 0.0f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public ChangeFogTask(DenseOrLight change){
		this.change = change;

		Debug.Assert(this.change == DenseOrLight.Dense || 
					 this.change == DenseOrLight.Light,
					 "Illegal change in fog: " + this.change);

		switch (this.change){
			case DenseOrLight.Dense:
				startDensity = LOW_FOG_DENSITY;
				endDensity = HIGH_FOG_DENSITY;
				break;
			case DenseOrLight.Light:
				startDensity = HIGH_FOG_DENSITY;
				endDensity = LOW_FOG_DENSITY;
				break;
		}
	}


	/// <summary>
	/// Each frame, increase or decrease the fog. Declare success when the timer exceeds changeTime.
	/// </summary>
	public override void Tick (){
		timer += Time.deltaTime;

		RenderSettings.fogDensity = Mathf.Lerp(startDensity, endDensity, timer/changeTime);

		if (timer > changeTime) SetStatus(TaskStatus.Success);
	}
}
