using MIConvexHull;
using UnityEngine;

public class RoomNode : IVertex
{
    public double[] Position { get; }
    public Room RoomData;

    public RoomNode(Room room)
    {
        RoomData = room;
        Position = new double[] { room.position.x, room.position.y };
    }
}