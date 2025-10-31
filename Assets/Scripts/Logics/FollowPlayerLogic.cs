using UnityEngine;

public class FollowPlayerLogic
{
    private readonly float _leftXBound;
    private readonly float _cameraY;
    private readonly float _cameraZ;

    public FollowPlayerLogic(float leftXBound = 3.5f, float cameraY = 0f, float cameraZ = -10f)
    {
        _leftXBound = leftXBound;
        _cameraY = cameraY;
        _cameraZ = cameraZ;
    }

    public Vector3 CalculateTargetPosition(Vector3 playerPosition)
    {
        float x = Mathf.Max(playerPosition.x, _leftXBound);
        return new Vector3(x, _cameraY, _cameraZ);
    }
}
