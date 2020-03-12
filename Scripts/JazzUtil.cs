using UnityEngine;

public class JazzUtil
{
    public static float SquareDistance(Vector3 a, Vector3 b)
    {
        float diffX = a.x - b.x;
        float diffY = a.x - b.x;
        float diffZ = a.x - b.x;
        return diffX * diffX + diffY + diffY + diffZ + diffZ;
    }
}