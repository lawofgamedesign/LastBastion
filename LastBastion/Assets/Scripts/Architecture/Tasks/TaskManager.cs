/// <summary>
/// Basic task system.
/// </summary>
using System.Collections.Generic;
using UnityEngine;

public class TaskManager {

	//current tasks
	private readonly List<Task> tasks = new List<Task>();


	/// <summary>
	/// Add a task to the list of tasks to address.
	/// </summary>
	/// <param name="task">Task.</param>
	public void AddTask(Task task){
		tasks.Add(task);
		task.SetStatus(Task.TaskStatus.Pending);
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
}
