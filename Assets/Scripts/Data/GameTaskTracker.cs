using System.Collections.Generic;
using UnityEngine;

public class GameTaskTracker : MonoBehaviour
{
    public static GameTaskTracker Instance { get; private set; }

    [System.Serializable]
    public class TaskRecord
    {
        public string taskId;
        public string description;
        public bool isCompleted;
    }

    // Stores all tasks by a unique taskId (e.g. "pickpocket_id", "enter_kitchen")
    readonly Dictionary<string, TaskRecord> taskRecords = new Dictionary<string, TaskRecord>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Registers a task as active (not yet completed) and updates the UI TaskListManager if present.
    public static void StartTask(string taskId, string description, bool showPopup = true)
    {
        if (Instance == null)
        {
            var go = new GameObject("GameTaskTracker");
            go.AddComponent<GameTaskTracker>();
        }

        if (!Instance.taskRecords.ContainsKey(taskId))
        {
            Instance.taskRecords[taskId] = new TaskRecord
            {
                taskId = taskId,
                description = description,
                isCompleted = false
            };

            if (TaskListManager.Instance != null)
                TaskListManager.Instance.SyncFromTracker(showPopup);
        }
    }

    public static void CompleteTask(string taskId, bool showPopup = false)
    {
        if (Instance == null) return;

        if (Instance.taskRecords.TryGetValue(taskId, out TaskRecord record))
        {
            if (!record.isCompleted)
            {
                record.isCompleted = true;

                if (TaskListManager.Instance != null)
                    TaskListManager.Instance.SyncFromTracker(showPopup);
            }
        }
    }

    // Checks whether a specific task has been completed.
    public static bool IsTaskCompleted(string taskId)
    {
        if (Instance == null || string.IsNullOrEmpty(taskId))
            return false;

        return Instance.taskRecords.TryGetValue(taskId, out TaskRecord record) && record.isCompleted;
    }

    // Returns all active and completed tasks (useful if rebuilding the UI Task List in a new scene).
    public static IEnumerable<TaskRecord> GetAllTasks()
    {
        if (Instance == null)
            return System.Array.Empty<TaskRecord>();

        return Instance.taskRecords.Values;
    }
}