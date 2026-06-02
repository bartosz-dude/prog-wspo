//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data.Test
{
  [TestClass]
  public class BallUnitTest
  {
    [TestMethod]
    public void ConstructorTestMethod()
    {
      Vector testinVector = new Vector(0.0, 0.0);
      Ball newInstance = new(0, testinVector, testinVector);
    }

    [TestMethod]
    public void MoveDistanceInTimeMethod()
    {
      Vector initialPosition = new Vector(10.0, 10.0);
      Vector velocity = new Vector(1.0, 0.0);

      Ball testingBall = new Ball(0, initialPosition, velocity);

      Thread.Sleep(1000);

      testingBall.Move();

      IVector finalPosition = testingBall.Position;

      double expectedDeltaX = 1.0 * 1.0;
      double expectedFinalX = initialPosition.x + expectedDeltaX;

      double acceptableDelta = 3.0;

      Assert.AreEqual(expectedFinalX, finalPosition.x, acceptableDelta,
          $"KBall did not move in real time. Expected around {expectedFinalX}, got {finalPosition.x}");

      Assert.AreEqual(initialPosition.y, finalPosition.y, 0.001,
      "Ball moved in Y when Y velocity is 0");
    }

    [TestMethod]
    public void MoveTestMethod()
    {
      Vector initialPosition = new(10.0, 10.0);
      Ball newInstance = new(0, initialPosition, new Vector(0.0, 0.0));
      IVector curentPosition = new Vector(0.0, 0.0);
      int numberOfCallBackCalled = 0;
      newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); curentPosition = position; numberOfCallBackCalled++; };
      newInstance.Move();
      Assert.AreEqual<int>(1, numberOfCallBackCalled);
      Assert.AreEqual<IVector>(initialPosition, curentPosition);
    }
  }
}