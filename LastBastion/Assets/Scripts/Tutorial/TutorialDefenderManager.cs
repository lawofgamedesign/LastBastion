namespace Tutorial
{
	using System.Collections.Generic;
	using UnityEngine;

	public class TutorialDefenderManager : DefenderManager {

		//initialize variables
		public override void Setup(){
			defenderOrganizer = GameObject.Find(DEFENDER_ORGANIZER).transform;
			spawnPoints = CreateSpawnPoints();
			defenders = MakeProtagonists(new DefenderTypes[] { DefenderTypes.Brawler });
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
		protected override List<TwoDLoc> CreateSpawnPoints(){
			List<TwoDLoc> temp = new List<TwoDLoc>() { spawn2 };

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
	}
}
