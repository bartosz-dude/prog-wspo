//____________________________________________________________________________________________________________________________________
//
//  Copyright (C) 2024, Mariusz Postol LODZ POLAND.
//
//  To be in touch join the community by pressing the `Watch` button and get started commenting using the discussion panel at
//
//  https://github.com/mpostol/TP/discussions/182
//
//_____________________________________________________________________________________________________________________________________

namespace TP.ConcurrentProgramming.Data
{
  internal class Ball : IBall
  {
    #region ctor

    public double Radius { get; } = 10.0;
    public double Weight { get; } = 10.0;
    public IVector Velocity { get; set; }
    public IVector Position { get; set; }

    internal Ball(Vector initialPosition, Vector initialVelocity)
    {
      Position = initialPosition;
      Velocity = initialVelocity;
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
      Position = new Vector(Position.x + Velocity.x, Position.y + Velocity.y);
      RaiseNewPositionChangeNotification();
    }

    #endregion private
  }
}