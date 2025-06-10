using UnityEngine;
using System;

[Serializable]
public class SirVector
{
    public float x;
    public float y;

    public SirVector(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public SirVector(Vector2 vector)
    {
        this.x = vector.x;
        this.y = vector.y;
    }

    public Vector2 GetVector()
    {
        return new Vector2(x, y);
    }
}
