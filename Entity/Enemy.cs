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

        // Collision detection between this enemy and a bullet
        public bool Intersects(Bullet bullet)
        {
            Rectangle enemyRect = new Rectangle(X, Y, Texture.Width, Texture.Height);
            Rectangle bulletRect = new Rectangle(bullet.X, bullet.Y, bullet.Texture.Width, bullet.Texture.Height);
            return Raylib.CheckCollisionRecs(enemyRect, bulletRect);
        }
    }
}
