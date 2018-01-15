using UnityEngine;

public class MoveEvent : Event {


	//the object that moved
	public readonly Transform movingObj;


	//the location the object moved to
	public readonly TwoDLoc endPos;


	//constructor
	public MoveEvent(Transform movingObj, TwoDLoc endPos){
		this.movingObj = movingObj;

		this.endPos = new TwoDLoc(endPos.x, endPos.z);
	}
}
