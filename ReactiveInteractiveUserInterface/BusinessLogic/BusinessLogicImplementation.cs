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
using UnderneathLayerAPI = TP.ConcurrentProgramming.Data.DataAbstractAPI;

namespace TP.ConcurrentProgramming.BusinessLogic
{
  internal class BusinessLogicImplementation : BusinessLogicAbstractAPI
  {


    #region ctor

    public BusinessLogicImplementation() : this(null)
    { }

    internal BusinessLogicImplementation(UnderneathLayerAPI? underneathLayer)
    {
      layerBellow = underneathLayer == null ? UnderneathLayerAPI.GetDataLayer() : underneathLayer;
    }

    #endregion ctor

    #region BusinessLogicAbstractAPI

    public override void Dispose()
    {
      if (Disposed)
        throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
      layerBellow.Dispose();
      Disposed = true;
    }

    private readonly object _collisionLock = new object();
    private readonly List<Data.IBall> _dataBalls = new();

    public override void Start(int numberOfBalls, Action<IPosition, IBall> upperLayerHandler)
    {
      var dims = GetDimensions;

      layerBellow.Start(numberOfBalls, dims.TableWidth, dims.TableHeight, (startingPos, dataBall) =>
      {
        lock (_collisionLock)
        {
          _dataBalls.Add(dataBall);
        }

        Ball bussinessBall = new Ball(dataBall);

        dataBall.NewPositionNotification += (sender, pos) =>
        {
          HandleBallLogic((Data.IBall)sender);
        };

        upperLayerHandler(new Position(startingPos.x, startingPos.y), bussinessBall);
      });
      // if (Disposed)
      //   throw new ObjectDisposedException(nameof(BusinessLogicImplementation));
      // if (upperLayerHandler == null)
      //   throw new ArgumentNullException(nameof(upperLayerHandler));
      // layerBellow.Start(numberOfBalls, (startingPosition, databall) => upperLayerHandler(new Position(startingPosition.x, startingPosition.x), new Ball(databall)));
    }

    private void HandleBallLogic(Data.IBall ball)
    {
      lock (_collisionLock)
      {
        CheckWallCollision(ball);
        CheckBallCollision(ball);
      }
    }

    private void CheckWallCollision(Data.IBall ball)
    {
      var dims = GetDimensions;
      double diameter = ball.Radius * 2;

      // horizontal box collision
      if (ball.Position.x <= 0 && ball.Velocity.x < 0)
      {
        ball.Velocity = new Data.Vector(-ball.Velocity.x, ball.Velocity.y);
      }
      else if (ball.Position.x + diameter >= dims.TableWidth && ball.Velocity.x > 0)
      {
        ball.Velocity = new Data.Vector(-ball.Velocity.x, ball.Velocity.y);
      }

      // vertical box collision
      if (ball.Position.y <= 0 && ball.Velocity.y < 0)
      {
        ball.Velocity = new Data.Vector(ball.Velocity.x, -ball.Velocity.y);
      }
      else if (ball.Position.y + diameter >= dims.TableHeight && ball.Velocity.y > 0)
      {
        ball.Velocity = new Data.Vector(ball.Velocity.x, -ball.Velocity.y);
      }
    }
    private void CheckBallCollision(Data.IBall ball)
    {
      foreach (var other in _dataBalls)
      {
        if (ReferenceEquals(ball, other)) continue;

        double dx = ball.Position.x - other.Position.x;
        double dy = ball.Position.y - other.Position.y;
        double distance = Math.Sqrt(dx * dx + dy * dy);

        if (distance <= 20.0)
        {
          var temp = ball.Velocity;
          ball.Velocity = other.Velocity;
          other.Velocity = temp;
        }
      }
    }

    #endregion BusinessLogicAbstractAPI

    #region private

    private bool Disposed = false;

    private readonly UnderneathLayerAPI layerBellow;

    #endregion private

    #region TestingInfrastructure

    [Conditional("DEBUG")]
    internal void CheckObjectDisposed(Action<bool> returnInstanceDisposed)
    {
      returnInstanceDisposed(Disposed);
    }

    #endregion TestingInfrastructure
  }
}