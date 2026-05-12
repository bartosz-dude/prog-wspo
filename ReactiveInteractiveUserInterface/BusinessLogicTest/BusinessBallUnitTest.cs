//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System.Diagnostics;

namespace TP.ConcurrentProgramming.BusinessLogic.Test
{
  [TestClass]
  public class BallUnitTest
  {
    // TODO add check of how many times ball process was run
    [TestMethod]
    public void MoveTestMethod()
    {
      Console.WriteLine("MoveTestMethod");
      DataBallFixture dataBallFixture = new DataBallFixture();
      Ball newInstance = new(dataBallFixture);
      int numberOfCallBackCalled = 0;
      newInstance.NewPositionNotification += (sender, position) => { Assert.IsNotNull(sender); Assert.IsNotNull(position); numberOfCallBackCalled++; };
      dataBallFixture.Move();
      Assert.AreEqual<int>(1, numberOfCallBackCalled);
    }

    [TestMethod]
    public void AsyncHighPerformanceBallProcessCountTest()
    {
      int numberOfBalls = 1000;
      int testDurationMs = 10000;

      long[] ballInvocationCounts = new long[numberOfBalls];

      using CancellationTokenSource cts = new();
      List<DataBallFixture> balls = new();

      for (int i = 0; i < numberOfBalls; i++)
      {
        var ball = new DataBallFixture();

        int index = i;

        ball.NewPositionNotification += (sender, pos) =>
        {
          Interlocked.Increment(ref ballInvocationCounts[index]);
        };
        balls.Add(ball);
      }

      Stopwatch timer = Stopwatch.StartNew();

      List<Task> tasks = new();
      foreach (var newBall in balls)
      {
        tasks.Add(Task.Run(async () =>
        {
          while (!cts.Token.IsCancellationRequested)
          {
            newBall.Move();
            await Task.Delay(15);
          }
        }, cts.Token));
      }

      Thread.Sleep(testDurationMs);
      cts.Cancel();

      Task.WaitAll(tasks.ToArray(), 1000);
      timer.Stop();

      long totalInvocations = ballInvocationCounts.Sum();
      int ballsWithZeroCounts = ballInvocationCounts.Count(count => count == 0);
      double averageCount = ballInvocationCounts.Average();

      Console.WriteLine($"Total balls: {numberOfBalls}");
      Console.WriteLine($"Total logic cycles: {totalInvocations}");
      Console.WriteLine($"Average cycles per ball: {averageCount:F2}");
      Console.WriteLine($"Balls never processed: {ballsWithZeroCounts}");

      Assert.AreEqual(0, ballsWithZeroCounts, "Every ball should have been processed at least once.");

      double minExpected = (testDurationMs / 30.0);
      foreach (var count in ballInvocationCounts)
      {
        Assert.IsTrue(count > 0, "A ball was missed by the scheduler.");
      }
    }

    #region testing instrumentation

    private class DataBallFixture : Data.IBall
    {
      public double Radius { get; } = 20.0;
      public double Weight { get; } = 10.0;
      public Data.IVector Position { get; set; }
      public Data.IVector Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

      public event EventHandler<Data.IVector>? NewPositionNotification;

      internal void Move()
      {
        NewPositionNotification?.Invoke(this, new VectorFixture(0.0, 0.0));
      }
    }

    private class VectorFixture : Data.IVector
    {
      internal VectorFixture(double X, double Y)
      {
        x = X; y = Y;
      }

      public double x { get; init; }
      public double y { get; init; }
    }

    #endregion testing instrumentation
  }
}