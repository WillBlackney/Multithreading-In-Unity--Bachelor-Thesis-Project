using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using Unity.Mathematics;
using System.Diagnostics;

public class ThreadQueue : MonoBehaviour
{
    // Threading Enum Declaration
    public enum ThreadingSystem { None, OldSchool, OldSchoolPooled, JobSystem };

    // Properties + Component References
    #region
    [Header("General Properties")]
    public ThreadingSystem threadingSystem;
    public List<Action> functionsToRunInMainThread;

    [Header("Thread Pool Properties")]
    public int maxAllowedThreads;
    public int runningThreads;
    public List<Action> functionsToRunInChildThread;
    #endregion

    // Singleton Pattern
    #region
    public static ThreadQueue Instance;
    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            functionsToRunInMainThread = new List<Action>();
            functionsToRunInChildThread = new List<Action>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    // Create, Queue And Execute Threads + Functions
    #region
    private void Update()
    {
        UnityEngine.Debug.Log("ThreadQueue.Update() started...");
        runningThreads = Process.GetCurrentProcess().Threads.Count;

        // update ALWAYS runs in the main thread    
        // while we have queued functions awaiting threads/operation

        // Check Child threaded function queue
        while (functionsToRunInChildThread.Count > 0 && runningThreads < maxAllowedThreads)
        {
            Action queuedFunction = functionsToRunInChildThread[0];
            functionsToRunInChildThread.RemoveAt(0);

            // Run the function            
            if (queuedFunction != null)
            {
                StartThreadedFunction(queuedFunction);
            }
        }

        // Check Main threaded function queue
        while (functionsToRunInMainThread.Count > 0)
        {            
            Action queuedFunction = functionsToRunInMainThread[0];
            functionsToRunInMainThread.RemoveAt(0);

            // Run the function            
            if(queuedFunction != null)
            {
                queuedFunction.Invoke();
            }                
        }
    }
    public void StartThreadedFunction(Action someFunction)
    {
        Thread newChildThread = new Thread(new ThreadStart(someFunction));
        UnityEngine.Debug.Log("Active running thread count: " + runningThreads);
        newChildThread.Start();
    }
    public void QueueChildThreadedFunction(Action someFunction)
    {
        functionsToRunInChildThread.Add(someFunction);
        UnityEngine.Debug.Log("Queued functions in CHILD thread queue: " + functionsToRunInChildThread.Count);
    }
    public void QueueMainThreadFunction(Action someFunction)
    {
        functionsToRunInMainThread.Add(someFunction);
        UnityEngine.Debug.Log("Queued functions in MAIN thread queue: " + functionsToRunInChildThread.Count);
    }
    #endregion

    // Manipulate Zombie Position + Data
    #region
    public void CalculateNewZombiePosition(Zombie zombie, float moveY, Vector3 currentPos, float deltaTime)
    {
        // NOTE: this function should ONLY be executed on a child thread 

        // Calculate new position
        Vector3 newPosition = new Vector3(currentPos.x, currentPos.y + (moveY * deltaTime), currentPos.z);

        // Calculate new direction to move in (up or down?)
        float newMoveY = moveY;
        if (newPosition.y > 5f)
        {
            newMoveY = -math.abs(newMoveY);
        }
        if (newPosition.y < -5f)
        {
            newMoveY = +math.abs(newMoveY);
        }

        // Do a tough math function
        float value = 0f;
        for (int i = 0; i < 1000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }

        // Create new anon function for Unity API stuff
        Action moveZombieFunction = () =>
        {
            // Safe to call the Unity API like this
            zombie.transform.position = new Vector3(newPosition.x, newPosition.y, newPosition.z);
            zombie.moveY = newMoveY;
        };

        // Queue the anon method (TO BE DONE ON THE MAIN THREAD!!)
        QueueMainThreadFunction(moveZombieFunction);
    }
    #endregion
}
