namespace EscMenu
{
	using UnityEngine;

	public class EscMenuButtonRestart : MonoBehaviour {


		/////////////////////////////////////////////
		/// Fields
		/////////////////////////////////////////////


		//the game manager
		private const string GAME_MANAGER_OBJ = "Game managers";


		public void RestartGame(){
			GameObject.Find(GAME_MANAGER_OBJ).GetComponent<GameManager>().Restart();
		}
	}
}
