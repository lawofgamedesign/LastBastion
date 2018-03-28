/// <summary>
/// Basic task system.
/// </summary>
using System.Collections.Generic;
using UnityEngine;

public class TaskManager {

	//current tasks
	private readonly List<Task> tasks = new List<Task>();


	//sequenced tasks
	//private Dictionary<string, Queue<Task>> queuedTasks = new Dictionary<string, Queue<Task>>();
	private Dictionary<string, LinkedList<Task>> orderedTasks = new Dictionary<string, LinkedList<Task>>();
	private List<string> finishedKeys = new List<string>();
	private List<Task> tasksToOrder = new List<Task>();


	/// <summary>
	/// Add a task to the list of tasks to address.
	/// </summary>
	/// <param name="task">Task.</param>
	public void AddTask(Task task){
		tasks.Add(task);
		task.SetStatus(Task.TaskStatus.Pending);
	}


	/// <summary>
	/// Call this to add a new ordered task.
	/// </summary>
	/// <param name="task">The task to be added.</param>
	public void AddOrderedTask(Task task){
		tasksToOrder.Add(task);
	}


	/// <summary>
	/// Adds tasks that should be ordered to the linkedlist that will resolve them.
	/// 
	/// This adds from the back of the tasksToOrder list so that if new tasks are added at an unpredicted or undesired moment,
	/// they will not be removed--but also will not interfere with adding tasks this frame. The list is not cleared,
	/// so they will be added next frame.
	/// </summary>
	private void PrepareNewOrderedTasks(){
		for (int i = tasksToOrder.Count - 1; i >= 0; i--){
			if (orderedTasks.ContainsKey(tasksToOrder[i].GetType().ToString())) orderedTasks[tasksToOrder[i].GetType().ToString()].AddFirst(tasksToOrder[i]);
			else {
				LinkedList<Task> temp = new LinkedList<Task>();
				temp.AddFirst(tasksToOrder[i]);
				orderedTasks.Add(tasksToOrder[i].GetType().ToString(), temp);
			}
			tasksToOrder[i].SetStatus(Task.TaskStatus.Pending);
			tasksToOrder.Remove(tasksToOrder[i]);
		}
	}


	/// <summary>
	/// Work through all current tasks.
	/// </summary>
	public void Tick(){
		for (int i = tasks.Count - 1; i >= 0; --i){
			Task task = tasks[i];

			if (task.IsPending) { task.SetStatus(Task.TaskStatus.Working); }

			if (task.IsFinished) { 
				HandleCompletion(task, i);
			} else {
				task.Tick();

				if (task.IsFinished){
					HandleCompletion(task, i);
				}
			}
		}
			
		foreach (string key in orderedTasks.Keys){
			if (orderedTasks[key].Count > 0){
				Task task = orderedTasks[key].First.Value;

					if (task.IsPending) { task.SetStatus(Task.TaskStatus.Working); }

				if (task.IsFinished) { 
					HandleOrderedCompletion(task);
				} else {
					task.Tick();

					if (task.IsFinished){
						HandleOrderedCompletion(task);
					}
				}
			}
		}

		PrepareNewOrderedTasks();
		CleanUpQueues();
	}


	/// <summary>
	/// When a task is done, add the next task (if any) to the list of current tasks and then stop the current task.
	/// </summary>
	/// <param name="task">The task that is finishing.</param>
	/// <param name="taskIndex">The index of the ending task in the list of tasks.</param>
	private void HandleCompletion(Task task, int taskIndex){
		if (task.NextTask != null && task.IsSuccessful) { AddTask(task.NextTask); }

		tasks.RemoveAt(taskIndex);
		task.SetStatus(Task.TaskStatus.Detached);
	}


	/// <summary>
	/// Handle the completion of ordered tasks, by doing the following:
	/// 
	/// 1. Remove the now-completed task from the linked list of ordered tasks. The following task in the linked list now becomes first, and will be worked on.
	/// 2. If the now-completed task has a next task, add that to the beginning of the linked list. Now it becomes the task to be worked on.
	/// 3. If there are no more ordered tasks of the completed task's type, remove the key for that task.
	/// 4. Detach the completed task.
	/// </summary>
	/// <param name="task">The finished task.</param>
	private void HandleOrderedCompletion(Task task){
		orderedTasks[task.GetType().ToString()].RemoveFirst();
		if (task.NextTask != null && task.IsSuccessful) { orderedTasks[task.GetType().ToString()].AddFirst(task.NextTask); }
		if (orderedTasks[task.GetType().ToString()].Count <= 0) finishedKeys.Add(task.GetType().ToString());
		task.SetStatus(Task.TaskStatus.Detached);
	}


	private void CleanUpQueues(){
		if (finishedKeys.Count > 0){
			foreach (string key in finishedKeys){
				Debug.Assert(orderedTasks[key].Count == 0);
				orderedTasks.Remove(key);
			}

			finishedKeys.Clear();
		}
	}


	/// <summary>
	/// Determines whether there is already a task of a given type in the list of current tasks.
	/// </summary>
	/// <returns><c>true</c> if there is a task of the given task's type in the list, <c>false</c> otherwise.</returns>
	/// <param name="task">A task whose type you wish to check against.</param>
	public bool CheckForTaskOfType(Task task){
		if (tasks.Exists(e => e.GetType() == task.GetType())) return true;

		return false;
	}


	/// <summary>
	/// Also determines whether the list of current tasks includes a task of a given type, this time by comparing directly
	/// against a type.
	/// </summary>
	/// <returns><c>true</c> if there is at least one current task of the given type, <c>false</c> otherwise.</returns>
	/// <typeparam name="T">The type to check against.</typeparam>
	public bool CheckForTaskOfType<T>(){
		if (tasks.Exists(e => e.GetType() == typeof(T))) return true;

		return false;
	}


	/// <summary>
	/// Get the last of a given task. "Last," here, means a task of that type with nothing to do "Then."
	/// 
	/// IMPORTANT: this only works with tasks where there's only going to be one of them running at any given time,
	/// with any additional tasks of that type assigned as a NextTask and running sequentially thereafter. It won't work
	/// if there are ever tasks of the type running in parallel.
	/// </summary>
	/// <returns>The last task of the type.</returns>
	/// <typeparam name="T">The type of the task.</typeparam>
	public Task GetLastTaskOfType<T>(){
		foreach (Task task in tasks){
			if (task.GetType() == typeof(T)){
				if (task.NextTask == null){
					return task;
				}
			}
		}

		return null;
	}


	/// <summary>
	/// Get the currently-running task of a given type.
	/// 
	/// IMPORTANT: this only works reliably with tasks where there's only going to be one of them running at a given time.
	/// If there's more than one, it will return one of them unpredictably.
	/// </summary>
	/// <returns>The current task of the given type.</returns>
	/// <typeparam name="T">The type of task to find.</typeparam>
	public Task GetCurrentTaskOfType<T>(){
		foreach (Task task in tasks){
			if (task.GetType() == typeof(T)){
				if (task.IsWorking) return task;
			}
		}

		return null;
	}


	/// <summary>
	/// Insert a task between two other tasks.
	/// </summary>
	/// <param name="insertPoint">The task that comes before the new task.</param>
	/// <param name="newTask">The inserted task.</param>
	public void InsertTask(Task insertPoint, Task newTask){
		if (insertPoint.NextTask != null) newTask.Then(insertPoint.NextTask);

		insertPoint.Then(newTask);
	}


	/// <summary>
	/// Are any tasks running at all?
	/// </summary>
	/// <returns><c>true</c> if any tasks are running, <c>false</c> otherwise.</returns>
	public bool CheckForAnyTasks(){
		if (tasks.Count > 0 ) return true;
		else return false;
	}
}
