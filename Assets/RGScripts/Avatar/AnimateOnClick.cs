/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.
 * 
 * AnimateOnClick.cs Revision 1.3.1105.25
 * Offer animation override facility activated on click */

using UnityEngine;
using System.Collections;

public class AnimateOnClick : MonoBehaviour
{
    public string animDefault = "walk";
    public string animOverride = "run";
    public bool playGesture = false;
    public float duration = 6.0f;
    private bool isOverriding = false;
    private float elapsedInterval = 0.0f;

    void FixedUpdate()
    {
        if (isOverriding)
        {
            // Only override for specified time
            elapsedInterval += Time.deltaTime;
            if (elapsedInterval > duration)
            {
                elapsedInterval = 0;
                isOverriding = false;
                AnimateCharacter tpa = GameObject.FindGameObjectWithTag("Player").GetComponent<AnimateCharacter>();
                if (tpa != null)
                {
                    tpa.AnimOverride(animDefault);
                }
            }
        }
    }
    void OnMouseDown()
    {
        AnimateCharacter tpa = GameObject.FindGameObjectWithTag("Player").GetComponent<AnimateCharacter>();
        if (tpa != null)
        {
            if (playGesture)
            {
                // Playing a gesture overrides the Idle animation, so can be used to get an avatar to clap or do some other different action
                tpa.SetGestureLength(duration);
                tpa.PlayGesture(animOverride);
            }
            else
            {
                // Override the default animation with the named override animation
                tpa.AnimOverride(animOverride);
                isOverriding = true;
            }
        }
    }
}
