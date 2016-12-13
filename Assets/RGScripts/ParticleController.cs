/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * ParticleController.cs Revision 1.0.1103.01
 * Add particles to your scene with this handy script for controlling emission  */

using UnityEngine;
using System.Collections;

public class ParticleController : MonoBehaviour
{

    private bool isEmitting = false;
    private float emitTimer = 0.0f;
    public float emissionTime = 1.0f;
    public bool runOnTimer = false;
    public float timerInterval = 10.0f;
    private float offTimer = 0.0f;

    void Start()
    {
        if (runOnTimer)
            StartEmitting();
    }
    void FixedUpdate()
    {
        if (isEmitting)
        {
            emitTimer = emitTimer + Time.deltaTime;
            if (emitTimer > emissionTime)
            {
                StopEmitting();
            }
        }
		else if (runOnTimer)
		{
            offTimer = offTimer + Time.deltaTime;
            if (offTimer > timerInterval)
            {                
                StartEmitting();
            }
		}
    }

    public void StartEmitting()
    {
        offTimer = 0.0f;
        GetComponent<ParticleEmitter>().emit = true;
        isEmitting = true;
    }

    public void StopEmitting()
    {
        GetComponent<ParticleEmitter>().emit = false;
        emitTimer = 0.0f;
        offTimer = 0.0f;
        isEmitting = false;
    }
}
