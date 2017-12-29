using UnityEngine;

public class ChatUITestManager : MonoBehaviour {

	private ChatUI chat;
	public ChatUI Chat {
		get {
			Debug.Assert(chat != null, "No chat system. Are services being created out of order?");
			return chat;
		}
		set { chat = value; }
	}


	private void Start(){
		Services.Tasks = new TaskManager();
		Chat = new ChatUI();
		Chat.Setup();
	}


	private void Update(){
		Services.Tasks.Tick();
	}
}
