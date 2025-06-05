using UnityEngine;

public class Room
{
    public Vector2 position;
    public float width, height;
    public bool IsStartRoom = false;
    public bool IsEndRoom = false;

    public Room(Vector2 pos, float w, float h)
    {
        position = pos;
        width = w;
        height = h;
    }

    public Rect GetRect()
    {
        return new Rect(position.x - width / 2, position.y - height / 2, width, height);
    }
}

