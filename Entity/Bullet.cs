namespace SpaceInvasionGame.Entity
{
    using Raylib_cs;

    public class Bullet : GameEntity
    {
        private int speedY = 6;

        public Bullet(int x, int y, Texture2D texture)
            : base(x, y, texture)
        {
        }

        // Bullets travel straight up the screen.
        public override void Move()
        {
            Y -= speedY;
        }
    }
}
