public class Task {

	public enum TaskStatus : byte { Detached, Pending, Working, Success, Fail, Aborted }

	public TaskStatus Status { get; private set; }

	public bool IsDetacted { get { return Status == TaskStatus.Detached; } }
	public bool IsAttached { get { return Status != TaskStatus.Detached; } }
	public bool IsPending { get { return Status == TaskStatus.Pending; } }
	public bool IsWorking { get { return Status == TaskStatus.Working; } }
	public bool IsSuccessful { get { return Status == TaskStatus.Success; } }
	public bool IsFailed { get { return Status == TaskStatus.Fail; } }
	public bool IsAborted { get { return Status == TaskStatus.Aborted; } }
	public bool IsFinished { get { return Status == TaskStatus.Fail ||
										  Status == TaskStatus.Success ||
										  Status == TaskStatus.Aborted; } }


	internal void SetStatus(TaskStatus newStatus){
		if (Status == newStatus) { return; }

		Status = newStatus;

		switch (newStatus){
			case TaskStatus.Working:
				Init();
				break;
			case TaskStatus.Success:
				OnSuccess();
				Cleanup();
				break;
			case TaskStatus.Fail:
				OnFail();
				Cleanup();
				break;
			case TaskStatus.Aborted:
				OnAbort();
				Cleanup();
				break;
			case TaskStatus.Detached:
			case TaskStatus.Pending:
				break;
			default:
				break;
		}
	}


	protected virtual void Init() {}
	public virtual void Tick() {}
	protected virtual void Cleanup() {}


	protected virtual void OnSuccess() {}
	protected virtual void OnFail() {}
	protected virtual void OnAbort() {}


	public Task NextTask { get; private set; }


	public Task Then(Task task){
		NextTask = task;
		return task;
	}
}
