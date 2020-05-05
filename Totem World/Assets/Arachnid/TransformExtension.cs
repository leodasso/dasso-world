using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtension
{
    public static int pixelsPerUnit = 10;
    
    public static int ActiveChildCount(this Transform transform)
    {
        int c = 0;
        foreach (Transform t in transform)
        {
            if (t.gameObject.activeSelf) c++;
        }

        return c;
    }

    /// <summary>
    /// Translates the object using pixel units rather than world units
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="pixels"></param>
    public static void PixelTranslate(this Transform transform, Vector2 pixels)
    {
        
    }
}
