using Raylib_cs;
using System.Collections.Generic;

namespace SpaceInvasionGame.Entity
{
    public class Player : IPlayer
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        private int speed;
        private Texture2D texture;

        public Player(int x, int y, int speed, Texture2D texture)
        {
            X = x;
            Y = y;
            this.speed = speed;
            this.texture = texture;
        }

        public void Move()
        {
            if (Raylib.IsKeyDown(KeyboardKey.Left)) X -= speed;
            if (Raylib.IsKeyDown(KeyboardKey.Right)) X += speed;

            if (X < 0) X = 0;
            if (X > 736) X = 736;
        }

        public void Draw()
        {
            Raylib.DrawTexture(texture, X, Y, Color.White);
        }

        public void Shoot(List<Bullet> bullets, Texture2D bulletTexture)
        {
            bullets.Add(new Bullet(X + 28, Y, bulletTexture));
        }
    }
}
