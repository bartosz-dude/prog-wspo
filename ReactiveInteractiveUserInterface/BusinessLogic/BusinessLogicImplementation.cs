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
          Task.Run(async () => HandleBallLogic((Data.IBall)sender));
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
      // lock (_collisionLock)
      // {
      CheckWallCollision(ball);
      CheckBallCollision(ball);
      // }
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
      else if ((ball.Position.x + diameter) >= dims.TableWidth && ball.Velocity.x > 0)
      {
        ball.Velocity = new Data.Vector(-ball.Velocity.x, ball.Velocity.y);
      }

      // vertical box collision
      if (ball.Position.y <= 0 && ball.Velocity.y < 0)
      {
        ball.Velocity = new Data.Vector(ball.Velocity.x, -ball.Velocity.y);
      }
      else if ((ball.Position.y + diameter) >= dims.TableHeight && ball.Velocity.y > 0)
      {
        ball.Velocity = new Data.Vector(ball.Velocity.x, -ball.Velocity.y);
      }

      if (ball.Position.x < (-diameter) || ball.Position.x > (dims.TableWidth + diameter) ||
    ball.Position.y < (-diameter) || ball.Position.y > (dims.TableHeight + diameter))
      {
        Console.Error.WriteLine($"Ball outside of the walls " + $"Position: ({ball.Position.x:F2}, {ball.Position.y:F2}), " + $"Table Size: {dims.TableWidth}x{dims.TableHeight}");
        Environment.FailFast($"Ball outside of the walls " + $"Position: ({ball.Position.x:F2}, {ball.Position.y:F2}), " + $"Table Size: {dims.TableWidth}x{dims.TableHeight}");
      }
    }
    private void CheckBallCollision(Data.IBall ball)
    {
      foreach (var other in _dataBalls)
      {
        if (ReferenceEquals(ball, other)) continue;
        var firstLock = ball.GetHashCode() < other.GetHashCode() ? ball : other;
        var secondLock = ball.GetHashCode() < other.GetHashCode() ? other : ball;

        double dx = ball.Position.x - other.Position.x;
        double dy = ball.Position.y - other.Position.y;
        double distance = Math.Sqrt(dx * dx + dy * dy);
        double minDistance = ball.Radius + other.Radius;

        if (distance <= minDistance)
        {
          if (distance == 0) distance = 0.1;

          lock (firstLock)
          {
            lock (secondLock)
            {

              double overlap = minDistance - distance;
              double nx = dx / distance; // normal X
              double ny = dy / distance; // normal Y

              // overlap move
              double moveX = nx * overlap / 2.0;
              double moveY = ny * overlap / 2.0;

              ball.Position = new Data.Vector(ball.Position.x + moveX, ball.Position.y + moveY);
              other.Position = new Data.Vector(other.Position.x - moveX, other.Position.y - moveY);

              // relative velocity
              double vRelX = ball.Velocity.x - other.Velocity.x;
              double vRelY = ball.Velocity.y - other.Velocity.y;

              // velocity along normal
              double vRelNormal = vRelX * nx + vRelY * ny;

              // already moving away
              if (vRelNormal > 0) continue;

              // scalar impulse to transfer for velocity based on weight transfer
              // j = -(1 + e) * v_rel_dot_n / (1/m1 + 1/m2)
              double j = -(1 + 1.0) * vRelNormal;
              j /= (1.0 / ball.Weight + 1.0 / other.Weight);

              ball.Velocity = new Data.Vector(
                  ball.Velocity.x + (j * nx) / ball.Weight,
                  ball.Velocity.y + (j * ny) / ball.Weight
              );

              other.Velocity = new Data.Vector(
                  other.Velocity.x - (j * nx) / other.Weight,
                  other.Velocity.y - (j * ny) / other.Weight
              );
            }
          }
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