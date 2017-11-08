﻿/// <summary>
/// Every space has a property for its contents, if any.
/// </summary>
using UnityEngine;

public class SpaceBehavior : MonoBehaviour {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//does this space contain an attacker, a defender, or nothing? For checking categories
	public enum ContentType { Attacker, Defender, Spawn, None };
	public ContentType contentType;


	//the publicly-accessible contents, for direct comparison
	public GameObject Contents { get; set; }


	//where is this space, in world coordinates?
	public Vector3 WorldLocation { get; set; }


	//where is this space in the grid?
	public TwoDLoc GridLocation { get; set; }


	//is something luring attackers into this space?
	public bool Lure { get; set; }
}
