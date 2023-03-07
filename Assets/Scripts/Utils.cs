using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public static class Utils 
{
    // Checks if two objects (lilypad and obstacle) are at the right height to possible interact
    public static bool OverlapRange(float pos1, float length1, float pos2, float length2)
    {
        var x2 = pos1 + 0.5f * length1;
        var x1 = pos1 - 0.5f * length1;
        
        var y2 = pos2 + 0.5f * length2;
        var y1 = pos2 - 0.5f * length2;

        return x1 <= y2 && y1 <= x2;
    }

    public static bool Overlap2D(Vector3 pos1, Vector3 size1, Vector3 pos2, Vector3 size2)
    {
        return OverlapRange(pos1.x, size1.x, pos2.x, size2.x) &&
               OverlapRange(pos1.y, size1.y, pos2.y, size2.y);
    }

    public static List<Transform> DirectlyConnectedLilypads(Transform cur, List<Transform> lilypads, float jumpDist, bool includeCur = false)
    {
        var connected = new List<Transform>();
        
        if (includeCur)
            connected.Add(cur);

        foreach (var lilypad in lilypads)
        {
            if (lilypad != cur && Vector3.SqrMagnitude(lilypad.position - cur.position) <= jumpDist * jumpDist)
                connected.Add(lilypad);
        }

        return connected;
    }

    public static List<Transform> ConnectedLilypads(Transform cur, List<Transform> lilypads, float jumpDist)
    {
        var connected = new List<Transform> { cur };
        lilypads.Remove(cur);
        
        foreach (var lilypad in lilypads.ToList())
        {
            if (Vector3.SqrMagnitude(lilypad.position - cur.position) <= jumpDist * jumpDist)
            {
                lilypads.Remove(lilypad);
                connected.AddRange(ConnectedLilypads(lilypad, lilypads, jumpDist));
            }
        }

        return connected;
    }

    public static Vector3 SpriteBounds(Transform t)
    {
        return t.GetComponent<SpriteRenderer>().sprite.bounds.size;
    }
    
    public static Vector3 ColliderBounds(Transform t)
    {
        return t.GetComponent<Collider2D>().bounds.size;
    }

    public static Vector3 RandomDirection()
    {
        var x = Random.Range(-1f, 1f);
        var y = Random.Range(-1f, 1f);
        return (new Vector3(x, y, 0f)).normalized;
    }

    public static bool OnScreen(Vector3 pos, Vector3 bounds)
    {
        return pos.x >= -bounds.x && pos.x <= bounds.x && pos.y >= -bounds.y && pos.y <= bounds.y;
    }
    
    public static bool OnScreen(Vector3 pos, Vector3 bounds, Vector3 size)
    {
        var paddedBounds = bounds - 0.5f * size;
        return pos.x >= -paddedBounds.x && pos.x <= paddedBounds.x && pos.y >= -paddedBounds.y && pos.y <= paddedBounds.y;
    }

    public static string FormatScore(float scoreF, int zeroes, Color color)
    {
        var score = (int)scoreF;
        var len = score.ToString().Length;

        var final = new string('0', zeroes - len) + "<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + score;
        return final;
    }

    public static Vector3 ScreenToWorldPos(Vector2 pos)
    {
        // range from 0 to 1
        pos = pos / new Vector2(Screen.width, Screen.height);
        // range from -1 to 1
        pos = 2 * pos - Vector2.one;

        var mainCam = Camera.main;
        var worldPos = new Vector3(pos.x * mainCam.orthographicSize * mainCam.aspect, pos.y * mainCam.orthographicSize, 0);
        
        return worldPos;
    }

    public static T PickRandom<T>(this List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }
    
    public static T PickRandom<T>(this T[] array)
    {
        return array[Random.Range(0, array.Length)];
    }

    public static Vector3 Flatten(this Vector3 v)
    {
        return new Vector3(v.x, v.y, 0);
    }
    
    public static Vector3 ToVector3(this Vector2 v, float z)
    {
        return new Vector3(v.x, v.y, z);
    }
    
    public static float Round(float value, int digits = 0)
    {
        var mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }
    
    public static int GetRandomWeightedIndex(float[] weights)
    {
        // Get the total sum of all the weights.
        var weightSum = weights.Sum();

        // Step through all the possibilities, one by one, checking to see if each one is selected.
        var index = 0;
        var lastIndex = weights.Length - 1;
        while (index < lastIndex)
        {
            // Do a probability check with a likelihood of weights[index] / weightSum.
            if (Random.Range(0, weightSum) < weights[index])
            {
                return index;
            }
 
            // Remove the last item from the sum of total untested weights and try again.
            weightSum -= weights[index++];
        }
 
        // No other item was selected, so return very last index.
        return index;
    }

    public static Vector2 RandomOnPerimeter(Vector2 rect)
    {
        var i = GetRandomWeightedIndex(new[] { rect.x, rect.y, rect.x, rect.y });
        
        return i switch
        {
            0 => new Vector2(Random.Range(-rect.x, rect.x), rect.y),
            1 => new Vector2(rect.x, Random.Range(-rect.y, rect.y)),
            2 => new Vector2(Random.Range(-rect.x, rect.x), -rect.y),
            3 => new Vector2(-rect.x, Random.Range(-rect.y, rect.y)),
            _ => Vector2.zero
        };
    }
    
    public static Color LerpHSV(Color a, Color b, float t, bool shortest = true)
    {
        // convert from RGB to HSV
        Color.RGBToHSV(a, out var ahue, out var asat, out var aval);
        Color.RGBToHSV(b, out var bhue, out var bsat, out var bval);

        float hue, sat, val;

        if ((Mathf.Abs(ahue - bhue) < 0.5f && shortest) || (Mathf.Abs(ahue - bhue) > 0.5f && !shortest))
            hue = Mathf.LerpUnclamped(ahue, bhue, t);
        else if (ahue > bhue)
            hue = Mathf.LerpUnclamped(ahue, bhue + 1, t) % 1;
        else
            hue = Mathf.LerpUnclamped(ahue + 1, bhue, t) % 1;

        sat = Mathf.LerpUnclamped(asat, bsat, t);
        val = Mathf.LerpUnclamped(aval, bval, t);
 
        // convert back to RGB and return the color
        return Color.HSVToRGB(hue, sat, val);
    }

    public static Vector3 SetX(this Vector3 v, float x)
    {
        v.x = x;
        return v;
    }
    
    public static Vector3 SetY(this Vector3 v, float y)
    {
        v.y = y;
        return v;
    }
    
    public static Vector3 SetZ(this Vector3 v, float z)
    {
        v.z = z;
        return v;
    }

    public static Vector2 xy(this Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }
    
    public static Vector2 yz(this Vector3 v)
    {
        return new Vector2(v.y, v.z);
    }
    
    public static Vector2 xz(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }
    
    public static Vector3 ClampMagnitude(this Vector3 v, float min, float max)	
    {	
        double sm = v.sqrMagnitude;	
        if(sm > max * max) return v.normalized * max;	
        if(sm < min * min) return v.normalized * min;	
        return v;	
    }	
    public static Vector3 Rotate(this Vector3 v, float angle)	
    {	
        return Quaternion.AngleAxis(angle, Vector3.up) * v;	
    }

    public static Quaternion ShortestRotation(Quaternion from, Quaternion to)
    {
        return Quaternion.Inverse(from) * to;
    }
    
    public static Vector2 Rotate(this Vector2 v, float degrees) {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);
         
        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
    
    public static float DistFromLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        var lineDirection = lineEnd - lineStart;
        var lineLength = lineDirection.magnitude;
        lineDirection.Normalize();

        var toPoint = point - lineStart;
        var dot = Vector3.Dot(toPoint, lineDirection);
        if (dot <= 0)
        {
            return (point - lineStart).magnitude;
        }
        else if (dot >= lineLength)
        {
            return (point - lineEnd).magnitude;
        }
        else
        {
            Vector3 projection = lineStart + dot * lineDirection;
            return (point - projection).magnitude;
        }
    }
    
    public static Vector3 Inverse(this Vector3 v)
    {
        return v / v.sqrMagnitude;
    }

    public static float Lerp(float a, float b, float t)
    {
        return (1 - t) * a + t * b;
    }
}
