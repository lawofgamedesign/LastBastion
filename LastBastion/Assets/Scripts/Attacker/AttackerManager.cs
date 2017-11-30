﻿using System.Collections.Generic;
using UnityEngine;

public class AttackerManager {


	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//attackers the manager can create, and the transform they're parented to
	private const string EVIL_WARRIOR_OBJ = "Evil Warrior";
	private const string PETTY_WARLORD_OBJ = "Petty Warlord";
	private const string ARMORED_WARLORD_OBJ = "Armored Warlord";
	private const string SKILLED_WARLORD_OBJ = "Skilled Warlord";
	private const string ENRAGED_WARLORD_OBJ = "Enraged Warlord";
	private const string FAST_WARLORD_OBJ = "Fast Warlord";
	private Transform attackerOrganizer;
	private const string ATTACKER_ORGANIZER = "Attackers";


	//list of all enemies
	private List<AttackerSandbox> attackers = new List<AttackerSandbox>();


	//spawn points
	private TwoDLoc spawn1 = new TwoDLoc(1, BoardBehavior.BOARD_HEIGHT - 1);
	private TwoDLoc spawn2 = new TwoDLoc(4, BoardBehavior.BOARD_HEIGHT - 1);
	private TwoDLoc spawn3 = new TwoDLoc(7, BoardBehavior.BOARD_HEIGHT - 1);
	List<TwoDLoc> spawnPoints = new List<TwoDLoc>();
	private const string SPAWNER_OBJ = "Spawn point";


	//how long attackers take to move one space
	public float MoveSpeed { get; private set; }


	//the waves
	private List<Wave> waves = new List<Wave>();
	private int waveIndex = 0; //which wave in the list of waves the game is currently on


	//used to give each enemy a unique ID number
	private int enemyNum = 0;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables and create the first 
	public virtual void Setup(){
		spawnPoints = CreateSpawnPoints();
		attackerOrganizer = GameObject.Find(ATTACKER_ORGANIZER).transform;
		MoveSpeed = 5.0f;
		waves = new List<Wave>() {
			new Wave(new List<string>() { ENRAGED_WARLORD_OBJ }, 3),
			new Wave(new List<string>() { ARMORED_WARLORD_OBJ, SKILLED_WARLORD_OBJ, ENRAGED_WARLORD_OBJ }, 6),
			new Wave(new List<string>() { FAST_WARLORD_OBJ, ARMORED_WARLORD_OBJ }, 6)
		};
		attackers = SpawnNewAttackers();
	}


	/// <summary>
	/// Create spawn points by following these steps:
	/// 1. Create a list of all the spawn point locations for this map.
	/// 2. Load the spawn point from the Resources folder.
	/// 3. Instantiate a copy of the loaded spawn point, and put it in the first space in the list.
	/// 4. Repeat (3) for all the spawn point locations in the list.
	/// 5. Return the list of locations.
	/// </summary>
	/// <returns>The spawn points.</returns>
	private List<TwoDLoc> CreateSpawnPoints(){
		List<TwoDLoc> temp = new List<TwoDLoc>() { spawn1, spawn2, spawn3 };

		GameObject spawnPoint = Resources.Load<GameObject>(SPAWNER_OBJ);

		foreach (TwoDLoc point in temp){
			Services.Board.PutThingInSpace(MonoBehaviour.Instantiate<GameObject>(spawnPoint,
																				 Services.Board.GetWorldLocation(point.x, point.z),
																				 spawnPoint.transform.rotation,
																				 Services.Board.BoardOrganizer),
										   point.x,
										   point.z,
										   SpaceBehavior.ContentType.Spawn);
		}

		return temp;
	}



	/// <summary>
	/// Queries the board for open spawn points. If there are any, puts a warlord there.
	/// </summary>
	/// <returns>The warlords.</returns>
	public List<AttackerSandbox> SpawnNewAttackers(){
		List<AttackerSandbox> temp = attackers; //make a copy of the current attackers on the board

		foreach (TwoDLoc point in spawnPoints){
			if (Services.Board.GeneralSpaceQuery(point.x, point.z) != SpaceBehavior.ContentType.Attacker &&
				Services.Board.GeneralSpaceQuery(point.x - 1, point.z) != SpaceBehavior.ContentType.Attacker &&
				Services.Board.GeneralSpaceQuery(point.x, point.z - 1) != SpaceBehavior.ContentType.Attacker &&
				Services.Board.GeneralSpaceQuery(point.x + 1, point.z) != SpaceBehavior.ContentType.Attacker){

				temp.Add(MakeWarlord(ChooseWarlordType(), point.x, point.z));
				temp.AddRange(MakeRetinue(point.x, point.z));
			}
		}

		return temp;
	}


	/// <summary>
	/// Randomly select a warlord among those that can be spawned in a given wave.
	/// </summary>
	/// <returns>The warlord type's name.</returns>
	private string ChooseWarlordType(){
		return waves[waveIndex].warlordTypes[Random.Range(0, waves[waveIndex].warlordTypes.Count)];
	}


	/// <summary>
	/// Increase waveIndex, so that warlords are drawn from the next wave.
	/// </summary>
	/// <returns><c>true</c> if there was a new wave to go to, <c>false</c> otherwise.</returns>
	public bool GoToNextWave(){
		if (waveIndex < waves.Count - 1){
			waveIndex++;
			return true;
		} else return false;
	}


	/// <summary>
	/// Get the number of turns this wave lasts.
	/// </summary>
	/// <returns>The number of turns.</returns>
	public int GetTurnsThisWave(){
		return waves[waveIndex].Turns;
	}


	/// <summary>
	/// Make a warlord.
	/// </summary>
	/// <returns>The warlord's AttackerSandbox (or inheriting script).</returns>
	/// <param name="type">The type of warlord to make.</param>
	/// <param name="x">The x coordinate on the board where the warlord is to be placed.</param>
	/// <param name="z">The z coordinate on the board where the warlord is to be placed.</param>
	private AttackerSandbox MakeWarlord(string type, int x, int z){
		//start by getting any defender currently in the space out of there
		if (Services.Board.GeneralSpaceQuery(x, z) == SpaceBehavior.ContentType.Defender){
			GameObject defender = Services.Board.GetThingInSpace(x, z);
			Services.Board.TakeThingFromSpace(x, z);
			Services.Board.PutThingInSpace(defender, x, z - 1, SpaceBehavior.ContentType.Defender);


			defender.GetComponent<DefenderSandbox>().NewLoc(x, z - 1);
			Services.Tasks.AddTask(new MoveDefenderTask(defender.GetComponent<Rigidbody>(),
														defender.GetComponent<DefenderSandbox>().Speed,
														new System.Collections.Generic.List<TwoDLoc>() { new TwoDLoc(x, z),
														new TwoDLoc(x, z - 1)}));
		}


		Vector3 startLoc = Services.Board.GetWorldLocation(x, z) + new Vector3(0.0f, 0.0f, Services.Board.SpaceSize * 2); //the warlord starts off the board

		GameObject newWarlord = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(type),
																	  startLoc,
																	  Quaternion.identity,
																	  attackerOrganizer);

		Debug.Assert(newWarlord != null, "Failed to create warlord.");

		Services.Board.PutThingInSpace(newWarlord, x, z, SpaceBehavior.ContentType.Attacker);

		newWarlord.GetComponent<AttackerSandbox>().Setup();
		newWarlord.GetComponent<AttackerSandbox>().NewLoc(x, z);
		newWarlord.name = type + enemyNum;
		enemyNum++;
		Services.Tasks.AddTask(new MoveTask(newWarlord.transform, x, z, MoveSpeed)); //create the task that moves the warlord onto the board

		return newWarlord.GetComponent<AttackerSandbox>();
	}


	/// <summary>
	/// Make a retinue for a warlord by putting an Evil Warrior in the spaces to the west, south, and east of the warlord--if they're open.
	/// </summary>
	/// <returns>A list of retinue members created.</returns>
	/// <param name="x">The warlord's x coordinate on the board.</param>
	/// <param name="z">The warlord's z coordinate on the board.</param>
	private List<AttackerSandbox> MakeRetinue(int x, int z){
		List<AttackerSandbox> temp = new List<AttackerSandbox>();

		GameObject newRetinueMember = null;

		//try to make the retinue member to the west
		if (Services.Board.GeneralSpaceQuery(x - 1, z) == SpaceBehavior.ContentType.None){
			Vector3 startLoc = Services.Board.GetWorldLocation(x - 1, z) + new Vector3(0.0f, 0.0f, Services.Board.SpaceSize * 2); //they start off the board
			newRetinueMember = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(EVIL_WARRIOR_OBJ),
																	 startLoc,
																	 Quaternion.identity,	
																	 attackerOrganizer);

			Debug.Assert(newRetinueMember != null, "Failed to create retinue member.");

			Services.Board.PutThingInSpace(newRetinueMember, x - 1, z, SpaceBehavior.ContentType.Attacker);

			newRetinueMember.GetComponent<AttackerSandbox>().Setup();
			newRetinueMember.GetComponent<AttackerSandbox>().NewLoc(x - 1, z);
			newRetinueMember.name = newRetinueMember.name + enemyNum;
			enemyNum++;
			Services.Tasks.AddTask(new MoveTask(newRetinueMember.transform, x - 1, z, MoveSpeed)); //create the task that moves them onto the board

			temp.Add(newRetinueMember.GetComponent<AttackerSandbox>());
		}

		//try to make the retinue member to the south
		if (Services.Board.GeneralSpaceQuery(x, z - 1) == SpaceBehavior.ContentType.None){
			Vector3 startLoc = Services.Board.GetWorldLocation(x, z - 1) + new Vector3(0.0f, 0.0f, Services.Board.SpaceSize * 2); //they start off the board
			newRetinueMember = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(EVIL_WARRIOR_OBJ),
																	 startLoc,
																	 Quaternion.identity,
																	 attackerOrganizer);

			Debug.Assert(newRetinueMember != null, "Failed to create retinue member.");

			Services.Board.PutThingInSpace(newRetinueMember, x, z - 1, SpaceBehavior.ContentType.Attacker);

			newRetinueMember.GetComponent<AttackerSandbox>().Setup();
			newRetinueMember.GetComponent<AttackerSandbox>().NewLoc(x, z - 1);
			newRetinueMember.name = newRetinueMember.name + enemyNum;
			enemyNum++;
			Services.Tasks.AddTask(new MoveTask(newRetinueMember.transform, x, z - 1, MoveSpeed)); //create the task that moves them onto the board

			temp.Add(newRetinueMember.GetComponent<AttackerSandbox>());
		}


		//try to make the retinue member to the east
		if (Services.Board.GeneralSpaceQuery(x + 1, z) == SpaceBehavior.ContentType.None){
			Vector3 startLoc = Services.Board.GetWorldLocation(x + 1, z) + new Vector3(0.0f, 0.0f, Services.Board.SpaceSize * 2); //they start off the board
			newRetinueMember = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(EVIL_WARRIOR_OBJ),
																	 startLoc,
																	 Quaternion.identity,
																	 attackerOrganizer);

			Debug.Assert(newRetinueMember != null, "Failed to create retinue member.");

			Services.Board.PutThingInSpace(newRetinueMember, x + 1, z, SpaceBehavior.ContentType.Attacker);

			newRetinueMember.GetComponent<AttackerSandbox>().Setup();
			newRetinueMember.GetComponent<AttackerSandbox>().NewLoc(x + 1, z);
			newRetinueMember.name = newRetinueMember.name + enemyNum;
			enemyNum++;
			Services.Tasks.AddTask(new MoveTask(newRetinueMember.transform, x + 1, z, MoveSpeed)); //create the task that moves them onto the board

			temp.Add(newRetinueMember.GetComponent<AttackerSandbox>());
		}

		return temp;
	}


	/// <summary>
	/// Provides public access to the list of attackers.
	/// </summary>
	/// <returns>The list.</returns>
	public List<AttackerSandbox> GetAttackers(){
		return attackers;
	}


	public void PrepareAttackerMove(){
		List<AttackerSandbox> currentAttackers = Services.Board.GetOrderedAttackerList();

		foreach (AttackerSandbox attacker in currentAttackers){
			attacker.PrepareToMove();
		}
	}


	public void MoveAttackers(){
		List<AttackerSandbox> currentAttackers = Services.Board.GetOrderedAttackerList();

		foreach (AttackerSandbox attacker in currentAttackers){
			attacker.TryMove();
		}
	}


	public void EliminateAttacker(AttackerSandbox attacker){
		attackers.Remove(attacker);
	}


	/// <summary>
	/// Pulls all attackers off the board (both visually and within the grid logic).
	/// </summary>
	public void RemoveAllAttackers(){
		foreach (AttackerSandbox attacker in attackers) attacker.BeRemovedFromBoard();
		attackers.Clear();
	}


	/// <summary>
	/// Class for waves.
	/// 
	/// warlordTypes is the names of the warlords that appear in this wave, as they appear in the Resources folder.
	/// Turns is the number of turns in this wave.
	/// </summary>
	public class Wave {
		public List<string> warlordTypes;
		public int Turns { get; private set; }

		public Wave(List<string> warlordTypes, int turns){
			this.warlordTypes = warlordTypes;
			Turns = turns;
		}
	}
}
