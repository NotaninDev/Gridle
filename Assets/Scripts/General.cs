using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.InteropServices;
using String = System.String;
using Random = System.Random;

public class General : MonoBehaviour
{
    public static Random rand;

    [DllImport("__Internal")]
    public static extern bool CopyToClipboard(string str);

    void Awake()
    {
        rand = new Random();
        mouseRayCasted = false;
    }
    void FixedUpdate()
    {
        mouseRayCasted = false;
    }

    public static GameObject AddChild(GameObject parent, string name = null)
    {
        GameObject child = new GameObject();
        child.transform.parent = parent.transform;
        child.transform.localScale = Vector3.one;
        child.transform.localPosition = Vector3.zero;
        if (name != null && name.Length > 0)
        {
            child.name = name;
        }
        return child;
    }

    public static void Shuffle<T>(T[] array, int start = 0, int end = -1)
    {
        int originalEnd = end;
        if (end == -1)
        {
            end = array.Length;
        }
        if (start < 0 || start > end || end > array.Length)
        {
            Debug.LogWarning("Shuffle: index out of range");
            Debug.LogWarning(String.Format("length: {0}, start: {1}, end: {2}", array.Length, start, originalEnd));
            return;
        }
        for (int i = start; i < end; i++)
        {
            int n = rand.Next(i, end);
            T temp = array[n];
            array[n] = array[i];
            array[i] = temp;
        }
    }

    private static RaycastHit2D[] mouseRayHits;
    private static bool mouseRayCasted;
    private static void CastMouseRay()
    {
        if (!mouseRayCasted)
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseRayHits = Physics2D.RaycastAll(pos, Vector2.zero, 0.01f);
            mouseRayCasted = true;
        }
    }
    public static bool GetMouseHover(Transform transform)
    {
        CastMouseRay();
        foreach (RaycastHit2D hit in mouseRayHits)
        {
            if (hit.transform == transform) { return true; }
        }
        return false;
    }

    public static IEnumerator WaitEvent(UnityEvent unityEvent, float delay)
    {
        yield return new WaitForSeconds(delay);
        unityEvent.Invoke();
    }
}
