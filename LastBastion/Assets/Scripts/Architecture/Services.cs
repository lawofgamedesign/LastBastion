﻿using UnityEngine;

public static class Services {

	private static BoardBehavior board;
	public static BoardBehavior Board {
		get {
			Debug.Assert(board != null, "No board. Did you forget to create one?");
			return board;
		}
		set { board = value; }
	}


	private static AttackerManager attackers;
	public static AttackerManager Attackers {
		get {
			Debug.Assert(attackers != null, "No attacker manager. Did you forget to create one?");
			return attackers;
		}
		set { attackers = value; }
	}


	private static TurnManager rulebook;
	public static TurnManager Rulebook {
		get {
			Debug.Assert(rulebook != null, "No TurnManager. Did you forget to create one?");
			return rulebook;
		}
		set { rulebook = value; }
	}


	private static TaskManager tasks;
	public static TaskManager Tasks {
		get {
			Debug.Assert(tasks != null, "No task manager. Did you forget to create one?");
			return tasks;
		}
		set { tasks = value; }
	}


	private static AttackerDeck attackDeck;
	public static AttackerDeck AttackDeck {
		get {
			Debug.Assert(attackDeck != null, "No attacker deck. Did you forget to create one?");
			return attackDeck;
		}
		set { attackDeck = value; }
	}


	private static DefenderManager defenders;
	public static DefenderManager Defenders {
		get {
			Debug.Assert(defenders != null, "No defender manager. Did you forget to create one?");
			return defenders;
		}
		set { defenders = value; }
	}


	private static EventManager events;
	public static EventManager Events {
		get {
			Debug.Assert(events != null, "No event manager. Did you forget to create one?");
			return events;
		}
		set { events = value; }
	}


	private static InputManager inputs;
	public static InputManager Inputs {
		get {
			Debug.Assert(inputs != null, "No input manager. Did you forget to create one?");
			return inputs;
		}
		set { inputs = value; }
	}


	private static UIManager uI;
	public static UIManager UI {
		get {
			Debug.Assert(uI != null, "No UI manager. Did you forget to create one?");
			return uI;
		}
		set { uI = value; }
	}
}