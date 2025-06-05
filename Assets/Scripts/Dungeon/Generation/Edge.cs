class Edge
{
    public Room A, B;
    public float Weight;

    public Edge(Room a, Room b, float weight)
    {
        A = a;
        B = b;
        Weight = weight;
    }
}
