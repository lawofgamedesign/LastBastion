using UnityEngine;
using UnityEngine.UI;

public class ScrollChatTask : Task{

	/////////////////////////////////////////////
	/// Fields
	/////////////////////////////////////////////


	//the scrollbar that will move
	private readonly Scrollbar scroll;
	private const string CHAT_OBJ = "Chat UI";
	private const string WINDOW_OBJ = "Chat window";
	private const string SCROLLBAR_OBJ = "Scrollbar Vertical";


	//scroll speed
	private float speed = 1.0f;


	/////////////////////////////////////////////
	/// Functions
	/////////////////////////////////////////////


	//constructor
	public ScrollChatTask(){
		scroll = GameObject.Find(CHAT_OBJ).transform.Find(WINDOW_OBJ).Find(SCROLLBAR_OBJ).GetComponent<Scrollbar>();
	}


	/// <summary>
	/// Each frame, scroll the chat window. When it's fully showing the latest message, stop.
	/// 
	/// 
	/// IMPORTANT: For this to work, the "Number Of Steps" in the Scrollbar component must be zero.
	/// </summary>
	public override void Tick (){
		scroll.value -= speed * Time.deltaTime;

		if (scroll.value <= 0.0f) SetStatus(TaskStatus.Success);
	}
}
