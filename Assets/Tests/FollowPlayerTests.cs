using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FollowPlayerTests
{
   [Test]
    public void PlayerRightOfBound_ReturnsPlayerX()
    {
        var logic = new FollowPlayerLogic();
        var result = logic.CalculateTargetPosition(new Vector3(5, 0, 0));
        Assert.AreEqual(new Vector3(5, 0, -10), result);
    }

    [Test]
    public void PlayerLeftOfBound_ReturnsBoundX()
    {
        var logic = new FollowPlayerLogic();
        var result = logic.CalculateTargetPosition(new Vector3(-5, 0, 0));
        Assert.AreEqual(new Vector3(3.5f, 0, -10), result);
    }
}
