namespace SpaceInvasionGame.Entity
{
    // Descends straight down at a steady pace, no horizontal movement.
    public class StraightDiveMovement : IMovementStrategy
    {
        public void Move(Enemy enemy)
        {
            enemy.Y += enemy.SpeedX;
        }
    }
}
