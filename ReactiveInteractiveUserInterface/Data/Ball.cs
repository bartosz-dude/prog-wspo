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
  internal class Ball : IBall
  {
    #region ctor
    private readonly int _id;
    private readonly object _positionLock = new object();
    private IVector _position;
    private IVector _velocity;
    private Stopwatch _stopwatch = new Stopwatch();

    public double Radius { get; } = 10.0;
    public double Weight { get; } = 10.0;
    public int Id => _id;
    // public IVector Velocity { get; set; }
    // public IVector Position { get; set; }

    public IVector Position
    {
      get { lock (_positionLock) return _position; }
      set { lock (_positionLock) _position = value; }
    }

    public IVector Velocity
    {
      get { lock (_positionLock) return _velocity; }
      set { lock (_positionLock) _velocity = value; }
    }

    internal Ball(int id, Vector initialPosition, Vector initialVelocity)
    {
      _id = id;
      _position = initialPosition;
      _velocity = initialVelocity;
      _stopwatch.Start();
    }

    #endregion ctor

    #region IBall

    public event EventHandler<IVector>? NewPositionNotification;



    #endregion IBall

    #region private



    private void RaiseNewPositionChangeNotification()
    {
      NewPositionNotification?.Invoke(this, Position);
    }

    internal void Move()
    {
      double elapsedSeconds = _stopwatch.Elapsed.TotalSeconds;
      _stopwatch.Restart();

      if (elapsedSeconds > 0.1) elapsedSeconds = 0.015;

      lock (_positionLock)
      {
        double newX = _position.x + (_velocity.x * elapsedSeconds);
        double newY = _position.y + (_velocity.y * elapsedSeconds);
        _position = new Vector(newX, newY);
      }

      RaiseNewPositionChangeNotification();
    }

    #endregion private
  }
}