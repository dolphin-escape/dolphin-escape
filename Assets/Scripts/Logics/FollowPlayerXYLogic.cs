using UnityEngine;

public class FollowPlayerXYLogic
{
    public Vector3 CalculateClampedPosition(
        Vector3 playerPosition,
        float minX, float maxX,
        float minY, float maxY,
        float cameraZ)
    {
        float clampedX = Mathf.Clamp(playerPosition.x, minX, maxX);
        float clampedY = Mathf.Clamp(playerPosition.y, minY, maxY);
        return new Vector3(clampedX, clampedY, cameraZ);
    }
}
