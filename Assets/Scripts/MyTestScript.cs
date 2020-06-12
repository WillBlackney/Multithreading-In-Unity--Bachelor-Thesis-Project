using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using System.Collections.Generic;
using UnityEngine.Jobs;
using System;

public class MyTestScript : MonoBehaviour
{
    [Header("Inspector Fields")]    
    [SerializeField] private Transform zombiePrefab;
    [SerializeField] private Transform zombieParent;
    [SerializeField] private int zombiesToSpawn;

    [Header("Properties")]
    private List<Zombie> zombieList;
    private void Start()
    {
        // Instantiate x amount of zombies
        zombieList = new List<Zombie>();
        for (int i = 0; i < zombiesToSpawn; i++)
        {
            Transform zombieTransform = Instantiate(zombiePrefab, new Vector3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f)), Quaternion.identity);            
            zombieList.Add(new Zombie
            {
                transform = zombieTransform,
                moveY = UnityEngine.Random.Range(1f, 2f)
            });
            zombieTransform.SetParent(zombieParent);
            
        }
    }
    void Update()
    {
        // Start the timer
        float startTime = Time.realtimeSinceStartup;

        // use Job System Threading
        if (ThreadQueue.Instance.threadingSystem == ThreadQueue.ThreadingSystem.JobSystem)
        {
            // create empty native arrays to store data on zombies
            NativeArray<float3> positionArray = new NativeArray<float3>(zombieList.Count, Allocator.TempJob);
            NativeArray<float> moveYArray = new NativeArray<float>(zombieList.Count, Allocator.TempJob);

            // populate arrays with zombie data (their current position, and their movement speed)
            for(int i = 0; i < zombieList.Count; i++)
            {
                positionArray[i] = zombieList[i].transform.position;
                moveYArray[i] = zombieList[i].moveY;
            }                       

            // Create a new 'job' and pass the data in 
            // this custom struct moves ALL the zombies at once: no need to create one 'job' per zombie
            ZombieMovementStruct zombieMovementTask = new ZombieMovementStruct
            {
                deltaTime = Time.deltaTime,
                positionArray = positionArray,
                moveYArray = moveYArray,
            };            

            // Schedule the job, then cache the 'job handle' object for later            
            JobHandle jobHandle = zombieMovementTask.Schedule(zombieList.Count, 100);

            // Force the job to be completed ASAP: make job system prioritize this 1st
            jobHandle.Complete();            

            // job was performed on duplicate data, NOT the actual zombies
            // apply the calculations on duplicate data BACK to the actual zombies
            for (int i = 0; i < zombieList.Count; i++)
            {
                zombieList[i].transform.position = positionArray[i];
                zombieList[i].moveY = moveYArray[i];
            }            

            // Dispose the native arrays, failing to call 'Dispose' will leak memory.
            positionArray.Dispose();
            moveYArray.Dispose();
        }

        // Use 'Old School' threading system
        else if (ThreadQueue.Instance.threadingSystem == ThreadQueue.ThreadingSystem.OldSchool)
        {
            foreach (Zombie zombie in zombieList)
            {
                Vector3 zombiePos = new Vector3(zombie.transform.position.x, zombie.transform.position.y, zombie.transform.position.z);
                float deltaTime = Time.deltaTime;

                ThreadQueue.Instance.StartThreadedFunction(() =>
                { ThreadQueue.Instance.CalculateNewZombiePosition(zombie, zombie.moveY, zombiePos, deltaTime); });
            }
        }

        // Use 'Old School' threading system with a thread pool
        else if(ThreadQueue.Instance.threadingSystem == ThreadQueue.ThreadingSystem.OldSchoolPooled)
        {
            foreach (Zombie zombie in zombieList)
            {
                Vector3 zombiePos = new Vector3(zombie.transform.position.x, zombie.transform.position.y, zombie.transform.position.z);
                float deltaTime = Time.deltaTime;

                // Create new anon function for child thread Queue
                Action calculateNewZombiePositionFunction = () =>
                {
                    // Calculate new position
                    Vector3 newPosition = new Vector3(zombiePos.x, zombiePos.y + (zombie.moveY * deltaTime), zombiePos.z);

                    // Calculate new direction to move in (up or down?)
                    float newMoveY = zombie.moveY;
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
                    ThreadQueue.Instance.QueueMainThreadFunction(moveZombieFunction);
                };

                // Queue the anon function to be done on next available child thread
                ThreadQueue.Instance.QueueChildThreadedFunction(calculateNewZombiePositionFunction);
            }
        }

        // Dont use any multe threading system
        else if (ThreadQueue.Instance.threadingSystem == ThreadQueue.ThreadingSystem.None)
        {
            foreach (Zombie zombie in zombieList)
            {
                // Move zombie
                zombie.transform.position += new Vector3(0, zombie.moveY * Time.deltaTime);
                if (zombie.transform.position.y > 5f)
                {
                    zombie.moveY = -math.abs(zombie.moveY);
                }
                if (zombie.transform.position.y < -5f)
                {
                    zombie.moveY = +math.abs(zombie.moveY);
                }

                // do tough math function
                ToughMathFunction();                   
            }
        }        

        // Print the duration of the frame in milliseconds
        Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
    } 
    public void ToughMathFunction()
    {
        float value = 0f;

        for (int i = 0; i < 1000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }

}

[BurstCompile]
public struct PathfindingStruct : IJob
{
    public void Execute()
    {
        // tough math function
        float value = 0f;
        for(int i = 0; i < 1000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}

[BurstCompile]
public struct ZombieMovementStruct : IJobParallelFor
{
    // Properties + Fields
    public NativeArray<float3> positionArray;
    public NativeArray<float> moveYArray;
    [ReadOnly] public float deltaTime;

    // Interface requirments
    public void Execute(int index)
    {
        // Calculate the new position of the zombie at 'index'
        positionArray[index] += new float3(0, moveYArray[index] * deltaTime, 0f);

        // Did the zombie move off screen? If so, invert moveY value
        if (positionArray[index].y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (positionArray[index].y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }

        // Do a tough math function, for performance/stress testing and learning     
        float value = 0f;
        for (int i = 0; i < 1000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }               
    }
}

[BurstCompile]
public struct PathfindingStructParalellTransform : IJobParallelForTransform
{
    public NativeArray<float> moveYArray;
    [ReadOnly] public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        transform.position += new Vector3(0, moveYArray[index] * deltaTime, 0f);
        if (transform.position.y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (transform.position.y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }

        float value = 0f;
        for (int i = 0; i < 1000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}


