/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information.
 * 
 * AnimateTexture.cs Revision 1.0.1103.01
 * Animate a texture across the face of a material - useful for mock moving water effect  */

using UnityEngine;
using System.Collections;

public class AnimateTexture : MonoBehaviour
{
    // Rate of scroll in x/y
    public float scrollX = 0.01f;
    public float scrollY = 0.01f;
    // Scale modifier to texture
    public float scaleX = 3;
    public float scaleY = 3;

    void Start()
    {
        GetComponent<Renderer>().material.mainTextureScale = new Vector2(scaleX, scaleY);
    }

    void Update()
    {
        // scroll the texture
        float offsetX = Time.time * scrollX;
        float offsetY = Time.time * scrollY;
        GetComponent<Renderer>().material.mainTextureOffset = new Vector2(offsetX, offsetY);
    }
}
