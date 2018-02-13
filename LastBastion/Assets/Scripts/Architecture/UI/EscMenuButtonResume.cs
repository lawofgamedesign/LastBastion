namespace EscMenu
{
	using UnityEngine;

	public class EscMenuButtonResume : MonoBehaviour {

		public void ResumeGame(){
			Services.Events.Fire(new PauseEvent(PauseEvent.Pause.Unpause));
			gameObject.SetActive(false);
		}
	}
}
