using NUnit.Framework;
using UnityEngine;

public class FollowPlayerXYTests
{
    [Test]
    public void PlayerInsideBounds_ReturnsExactPosition()
    {
        var logic = new FollowPlayerXYLogic();
        var result = logic.CalculateClampedPosition(
            new Vector3(10, 20, 0),
            0, 100,
            0, 50,
            -10
        );

        Assert.AreEqual(new Vector3(10, 20, -10), result);
    }

    [Test]
    public void PlayerLeftAndBelowBounds_ReturnsMinValues()
    {
        var logic = new FollowPlayerXYLogic();
        var result = logic.CalculateClampedPosition(
            new Vector3(-5, -8, 0),
            0, 100,
            0, 50,
            -10
        );

        Assert.AreEqual(new Vector3(0, 0, -10), result);
    }

    [Test]
    public void PlayerRightAndAboveBounds_ReturnsMaxValues()
    {
        var logic = new FollowPlayerXYLogic();
        var result = logic.CalculateClampedPosition(
            new Vector3(120, 60, 0),
            0, 100,
            0, 50,
            -10
        );

        Assert.AreEqual(new Vector3(100, 50, -10), result);
    }
}
