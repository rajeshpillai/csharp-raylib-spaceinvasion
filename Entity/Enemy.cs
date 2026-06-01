namespace SpaceInvasionGame.Entity
{
    using Raylib_cs;

    public class Enemy : GameEntity
    {
        public int SpeedX { get; set; }
        public int SpeedY { get; set; }
        public bool MovingRight { get; set; } = true;
        private IMovementStrategy movementStrategy;

        public Enemy(int x, int y, int speedX, int speedY, Texture2D texture, IMovementStrategy strategy)
            : base(x, y, texture)
        {
            SpeedX = speedX;
            SpeedY = speedY;
            movementStrategy = strategy;
        }

        public override void Move()
        {
            movementStrategy.Move(this);
        }

        // Collision detection between this enemy and any other entity.
        public bool Intersects(GameEntity other)
        {
            return Raylib.CheckCollisionRecs(Bounds, other.Bounds);
        }
    }
}
