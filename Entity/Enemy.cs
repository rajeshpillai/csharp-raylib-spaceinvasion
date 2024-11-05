namespace SpaceInvasionGame.Entity
{
    using Raylib_cs;

    public class Enemy
    {
        public int X { get; set; }
        public int Y { get; set; }
        private int speedX;
        private int speedY;
        private bool movingRight = true;
        private Texture2D texture;

        public Enemy(int x, int y, int speedX, int speedY, Texture2D texture)
        {
            X = x;
            Y = y;
            this.speedX = speedX;
            this.speedY = speedY;
            this.texture = texture;
        }

        public void Move()
        {
            X += movingRight ? speedX : -speedX;
            if (X <= 0 || X >= 736)
            {
                movingRight = !movingRight;
                Y += speedY;
            }
        }

        public void Draw()
        {
            Raylib.DrawTexture(texture, X, Y, Color.White);
        }

        public bool Intersects(Bullet bullet)
        {
            Rectangle enemyRect = new Rectangle(X, Y, texture.Width, texture.Height);
            Rectangle bulletRect = new Rectangle(bullet.X, bullet.Y, bullet.Texture.Width, bullet.Texture.Height);
            return Raylib.CheckCollisionRecs(enemyRect, bulletRect);
        }
    }
}
