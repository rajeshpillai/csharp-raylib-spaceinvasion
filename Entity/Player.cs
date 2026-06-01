using Raylib_cs;
using System.Collections.Generic;

namespace SpaceInvasionGame.Entity
{
    public class Player : GameEntity, IPlayer
    {
        private int speed;

        public Player(int x, int y, int speed, Texture2D texture)
            : base(x, y, texture)
        {
            this.speed = speed;
        }

        public override void Move()
        {
            if (Raylib.IsKeyDown(KeyboardKey.Left)) X -= speed;
            if (Raylib.IsKeyDown(KeyboardKey.Right)) X += speed;

            if (X < 0) X = 0;
            if (X > 736) X = 736;
        }

        public void Shoot(List<Bullet> bullets, Texture2D bulletTexture)
        {
            bullets.Add(new Bullet(X + 28, Y, bulletTexture));
        }
    }
}
