using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SimHandlerScript : MonoBehaviour
{

    public DateTime simTime; //Epoch time of the live sim

    [Tooltip("Simulation Speed Multiplier")]
	[Range(-100f, 100f)]
	public float simSpeed; //Multiplier to speed up, slow down, or reverse sim

    // Start is called before the first frame update
    void Start()
    {
        // simTime = DateTime.Now;
        simTime = new DateTime(2023, 10, 30, 12, 18, 0);
    }

    // Update is called once per frame
    void Update()
    {
        simTime = simTime.AddSeconds(Time.deltaTime * simSpeed);
    }
}
