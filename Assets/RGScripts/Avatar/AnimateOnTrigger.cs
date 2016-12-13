/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.
 * 
 * AnimateOnTrigger.cs Revision 1.3.1105.25
 * Offer animation override facility activated on trigger */

using UnityEngine;
using System.Collections;

public class AnimateOnTrigger : MonoBehaviour
{
    public string animDefault = "walk";
    public string animOverride = "run";
    public bool playGesture = false;
    public float gestureDuration = 6.0f;

    void OnTriggerEnter(Collider other)
    {
        AnimateCharacter tpa = GameObject.FindGameObjectWithTag("Player").GetComponent<AnimateCharacter>();
        if (tpa != null)
        {
            if (playGesture)
            {
                // Playing a gesture overrides the Idle animation, so can be used to get an avatar to clap or do some other different action
                tpa.SetGestureLength(gestureDuration);
                tpa.PlayGesture(animOverride);
            }
            else
            {
                // Override the default animation with the named override animation
                tpa.AnimOverride(animOverride);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Reset the override on the default animation
        AnimateCharacter tpa = GameObject.FindGameObjectWithTag("Player").GetComponent<AnimateCharacter>();
        if (tpa != null)
        {
            tpa.AnimOverride(animDefault);
        }
    }
}
