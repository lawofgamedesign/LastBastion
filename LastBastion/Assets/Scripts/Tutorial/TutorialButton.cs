namespace Tutorial
{
	using UnityEngine;

	public class TutorialButton : MonoBehaviour {

		public void OnClick(){
			Services.Events.Fire(new TutorialClick());
		}
	}
}
