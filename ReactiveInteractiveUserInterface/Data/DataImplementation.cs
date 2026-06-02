//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

using System;
using System.Diagnostics;

namespace TP.ConcurrentProgramming.Data
{
  internal class DataImplementation : DataAbstractAPI
  {
    #region ctor

    public DataImplementation()
    {
      MoveTimer = new Timer(Move, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(100));
    }

    #endregion ctor

    #region DataAbstractAPI

    private CancellationTokenSource? _cts;
    private bool _disposed = false;
    private readonly List<Ball> _ballsList = new();
    private DiagnosticsLogger? _logger;

    public override void Start(int numberOfBalls, double width, double height, Action<IVector, IBall> upperLayerHandler)
    {
      _cts = new CancellationTokenSource();
      _logger = new DiagnosticsLogger();
      Random random = new Random();

      double radius = 10.0;
      double minDistance = radius * 2;

      for (int i = 0; i < numberOfBalls; i++)
      {
        Vector pos = new Vector(
          random.NextDouble() * (width - 2 * radius) + radius,
          random.NextDouble() * (height - 2 * radius) + radius
        );
        // Vector pos = new Vector(
        //     random.NextDouble() * (width - 25) + 10,
        //     random.NextDouble() * (height - 25) + 10
        // );
        Vector vel = new Vector(random.NextDouble() * 40 - 2, random.NextDouble() * 40 - 2);

        Ball newBall = new Ball(i, pos, vel);
        _ballsList.Add(newBall);

        upperLayerHandler(pos, newBall);

        Task.Run(async () =>
        {
          while (!_cts.Token.IsCancellationRequested)
          {
            newBall.Move();
            _logger?.LogBallState(newBall.Id, newBall.Position, newBall.Velocity);

            await Task.Delay(15);
          }
        });
      }
    }


    #endregion DataAbstractAPI

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
      if (!Disposed)
      {
        if (disposing)
        {
          MoveTimer.Dispose();
          BallsList.Clear();
        }
        Disposed = true;
      }
      else
        throw new ObjectDisposedException(nameof(DataImplementation));
    }

    public override void Dispose()
    {
      // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    #endregion IDisposable

    #region private

    //private bool disposedValue;
    private bool Disposed = false;

    private readonly Timer MoveTimer;
    private Random RandomGenerator = new();
    private List<Ball> BallsList = [];

    private void Move(object? x)
    {
      // foreach (Ball item in BallsList)
      // item.Move(new Vector((RandomGenerator.NextDouble() - 0.5) * 10, (RandomGenerator.NextDouble() - 0.5) * 10));
    }

    #endregion private

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckBallsList(Action<IEnumerable<IBall>> returnBallsList)
    {
      returnBallsList(BallsList);
    }

    [Conditional("DEBUG")]
    internal void CheckNumberOfBalls(Action<int> returnNumberOfBalls)
    {
      returnNumberOfBalls(BallsList.Count);
    }

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }
}