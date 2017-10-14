using System.Collections.Generic;
using UnityEngine;

public class DefenderManager {

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//list of all defenders
	private List<DefenderSandbox> defenders;


	//defenders the manager can create
	private GameObject genericDefender;
	public enum DefenderTypes { Generic_Defender, Guardian, Brawler, Ranger }
	private Transform defenderOrganizer;
	private const string DEFENDER_ORGANIZER = "Defenders";


	//spawn points
	private TwoDLoc spawn1 = new TwoDLoc(1, Services.Board.WallZPos);
	private TwoDLoc spawn2 = new TwoDLoc(4, Services.Board.WallZPos);
	private TwoDLoc spawn3 = new TwoDLoc(7, Services.Board.WallZPos);
	List<TwoDLoc> spawnPoints = new List<TwoDLoc>();
	private const string SPAWNER_OBJ = "Defender spawn point";


	//the currently selected defender
	private DefenderSandbox selectedDefender = null;



	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//initialize variables
	public void Setup(){
		defenderOrganizer = GameObject.Find(DEFENDER_ORGANIZER).transform;
		spawnPoints = CreateSpawnPoints();
		defenders = MakeProtagonists(new DefenderTypes[] { DefenderTypes.Generic_Defender, DefenderTypes.Generic_Defender, DefenderTypes.Generic_Defender });
	}


	/// <summary>
	/// Create spawn points by following these steps:
	/// 1. Create a list of all the spawn point locations for this map.
	/// 2. Load the spawn point from the Resources folder.
	/// 3. Instantiate a copy of the loaded spawn point, and put it in the first space in the list.
	/// 4. Repeat (3) for all the spawn point locations in the list.
	/// 5. Return the list of locations.
	/// 
	/// 
	/// This intentionally marks the space as empty; attacker spawns don't take up space or prevent movement.
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
																				 SpaceBehavior.ContentType.None);
		}

		return temp;
	}


	/// <summary>
	/// Create a list of all defenders.
	/// </summary>
	/// <returns>The list.</returns>
	/// <param name="defenderTypes">An array of defenders to create, listed by their in-game type (not c# class!).</param>
	private List<DefenderSandbox> MakeProtagonists(DefenderTypes[] defenderTypes){
		List<DefenderSandbox> temp = new List<DefenderSandbox>();

		Debug.Assert(defenderTypes.Length == spawnPoints.Count, "Mismatch between defenders to create and available spawn points.");

		for (int i = 0; i < spawnPoints.Count; i++){
			temp.Add(MakeDefender(defenderTypes[i], spawnPoints[i]));
		}

		return temp;
	}


	/// <summary>
	/// Create a single defender, and add it to the grid.
	/// </summary>
	/// <returns>The defender's controller script.</returns>
	/// <param name="defenderType">The in-game type of defender to create (not its c# class!).</param>
	/// <param name="spawnPoint">The spawn point where the defender will appear.</param>
	private DefenderSandbox MakeDefender(DefenderTypes defenderType, TwoDLoc spawnPoint){
		GameObject newDefender = MonoBehaviour.Instantiate<GameObject>(Resources.Load<GameObject>(defenderType.ToString()),
																	   Services.Board.GetWorldLocation(spawnPoint.x, spawnPoint.z),
																	   Quaternion.identity,
																	   defenderOrganizer);

		Services.Board.PutThingInSpace(newDefender, spawnPoint.x, spawnPoint.z, SpaceBehavior.ContentType.Defender);

		newDefender.GetComponent<DefenderSandbox>().Setup();
		newDefender.GetComponent<DefenderSandbox>().NewLoc(spawnPoint.x, spawnPoint.z);

		return newDefender.GetComponent<DefenderSandbox>();
	}


	/// <summary>
	/// Selects a defender for movement or attacking. Players cannot select a new defender while one is in the middle of a move.
	/// </summary>
	/// <param name="selected">The selected defender.</param>
	public void SelectDefender(DefenderSandbox selected){
		if (selected.Selected) return; //if the player is re-selecting the already-selected defender, do nothing

		//if a defender is in the middle of a move, stop
		foreach (DefenderSandbox defender in defenders){
			if (defender.IsMoving()) return;
		}


		//de-select everyone
		foreach (DefenderSandbox defender in defenders){
			defender.BeUnselected();
		}

		//select the chosen defender
		selectedDefender = selected;
		selected.BeSelected();
	}


	/// <summary>
	/// Check to see if any defenders are selected.
	/// </summary>
	/// <returns><c>true</c> if there is a currently selected defender, <c>false</c> if not.</returns>
	public bool IsAnyoneSelected(){
		bool temp = false;

		foreach (DefenderSandbox defender in defenders){
			if (defender.Selected) temp = true;
		}

		return temp;
	}


	/// <summary>
	/// Publicly-accessible way to get the currently selected defender.
	/// 
	/// This will return null if there is no currently selected defender; it's up to the calling function to check for null returns.
	/// </summary>
	/// <returns>The selected defender.</returns>
	public DefenderSandbox GetSelectedDefender(){
		return selectedDefender;
	}


	/// <summary>
	/// Walks through each defender, and sets them up for the move phase.
	/// </summary>
	public void PrepareDefenderMovePhase(){
		foreach (DefenderSandbox defender in defenders) defender.PrepareToMove();
	}
}
