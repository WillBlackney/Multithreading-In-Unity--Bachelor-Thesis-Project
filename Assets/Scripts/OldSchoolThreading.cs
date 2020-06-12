using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Unity.Mathematics;

public class OldSchoolThreading : MonoBehaviour
{
    [Header("Properties + Component References")]   
    public Thread myThread;

    void Start()
    {
        Debug.Log("Start() starting...");

        myThread = new Thread(SlowJob);
        myThread.Start();

        Debug.Log("Start() finishing...");
    }

    void Update()
    {
        if (myThread.IsAlive)
        {
            Debug.Log("SlowJob() is running...");
        }
    }

    void SlowJob()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        Debug.Log("SlowJob() called, running tough math function...");

        for(int i = 0; i < 1000; i++)
        {
            transform.Translate(Vector3.up * 0.002f);
        }        

        //ToughMathFunction();

        Debug.Log("SlowJob() finished, duration: " + sw.ElapsedMilliseconds/1000f + " seconds");
    }

    public void ToughMathFunction()
    {
        float value = 0f;

        for (int i = 0; i < 10000000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}
