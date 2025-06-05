using UnityEngine;

public class Corridor
{
    public Vector2 start;
    public Vector2 end;
    public float width;
    public Room A { get; private set; }
    public Room B { get; private set; }

    public Corridor(Room a, Room b, Vector2 start, Vector2 end, float width = 2f)
    {
        this.A = a;
        this.B = b;
        this.start = start;
        this.end = end;
        this.width = width;
    }
}
