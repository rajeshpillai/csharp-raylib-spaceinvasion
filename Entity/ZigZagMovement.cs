namespace SpaceInvasionGame.Entity
{
    public class ZigZagMovement : IMovementStrategy
    {
        public void Move(Enemy enemy)
        {
            enemy.X += enemy.MovingRight ? enemy.SpeedX : -enemy.SpeedX;
            if (enemy.X <= 0 || enemy.X >= 736)
            {
                enemy.MovingRight = !enemy.MovingRight;
                enemy.Y += enemy.SpeedY;
            }
        }
    }
}
