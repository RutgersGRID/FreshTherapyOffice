/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * MiniMap.cs Revision 1.0.1103.01
 * Ensure the minimap camera is the correct size and aspect ratio, and present user with a series of options for where to dock the minimap on screen */

using UnityEngine;
using System.Collections;

public enum MapAnchor { TopLeft, TopRight, BottomLeft, BottomRight, TopCenter };

public class MiniMap : MonoBehaviour 
{

    public bool enableMiniMap = true;
    public float mapWidthPixels = 100.0f;
    public float mapHeightPixels = 100.0f;
    
    private float normalizedWidth = 0.0f;
    private float normalizedHeight = 0.0f;

    private float normalizedLeftX = 0.0f;
    private float normalizedRightX = 0.0f;
    private float normalizedBottomY = 0.0f;
    private float normalizedTopY = 0.0f;
    private float normalizedOffsetCenterX = 0.0f;

    private float anchorX = 0.0f;
    private float anchorY = 0.0f;

    public MapAnchor mapAnchorPoint = MapAnchor.TopLeft;
    public float edgePadding = 2.0f;

    void Awake()
    {
        CalculateCamera();
    }
    void Update()
    {
        CalculateCamera();
    }

    private void CalculateCamera()
    {
        // A camera is drawn on the ViewPort coordinate space, where values go from 0 to 1, from nothing to 
        // whole screen - a bit like percentage of screen width and height.
        // Therefore, to calculate a fixed pixel size camera, some math is required!
        normalizedWidth = mapWidthPixels / Screen.width;
        normalizedHeight = mapHeightPixels / Screen.height;

        normalizedRightX = (Screen.width - (mapWidthPixels + edgePadding)) / Screen.width;
        normalizedOffsetCenterX = ((Screen.width / 2) - (mapWidthPixels / 2)) / Screen.width;
        normalizedTopY = (Screen.height - (mapHeightPixels + edgePadding)) / Screen.height;
        normalizedBottomY = 0.0f;
        normalizedLeftX = edgePadding / Screen.width;

        switch (mapAnchorPoint)
        {
            case MapAnchor.BottomLeft:
                anchorX = normalizedLeftX;
                anchorY = normalizedBottomY;
                break;
            case MapAnchor.BottomRight:
                anchorX = normalizedRightX;
                anchorY = normalizedBottomY;
                break;
            case MapAnchor.TopLeft:
                anchorX = normalizedLeftX;
                anchorY = normalizedTopY;
                break;
            case MapAnchor.TopRight:
                anchorX = normalizedRightX;
                anchorY = normalizedTopY;
                break;
            case MapAnchor.TopCenter:
                anchorX = normalizedOffsetCenterX;
                anchorY = normalizedTopY;
                break;
            default:
                break;
        }
        GetComponent<Camera>().rect = new Rect(anchorX, anchorY, normalizedWidth, normalizedHeight);
    }

    public float GetMapWidth()
    {
        return mapWidthPixels;
    }
    public float GetMapHeight()
    {
        return mapHeightPixels;
    }

    public MapAnchor GetMapAnchor()
    {
        return mapAnchorPoint;
    }

    public float GetMapPadding()
    {
        return edgePadding;
    }
}
