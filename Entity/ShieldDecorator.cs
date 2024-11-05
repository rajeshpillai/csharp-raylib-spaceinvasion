using Raylib_cs;
using System;
using System.Collections.Generic;

namespace SpaceInvasionGame.Entity
{
    public class ShieldDecorator : IPlayer
    {
        private IPlayer player;
        private Texture2D shieldTexture;
        private bool shieldActive;
        private float shieldDuration;
        private DateTime shieldActivatedTime;

        public int X => player.X;
        public int Y => player.Y;

        public ShieldDecorator(IPlayer player, Texture2D shieldTexture, float duration = 5.0f)
        {
            this.player = player;
            this.shieldTexture = shieldTexture;
            this.shieldDuration = duration;
            shieldActive = false;
        }

        public void ActivateShield()
        {
            shieldActive = true;
            shieldActivatedTime = DateTime.Now;
        }

        private void CheckShieldStatus()
        {
            if (shieldActive && (DateTime.Now - shieldActivatedTime).TotalSeconds >= shieldDuration)
            {
                shieldActive = false;
            }
        }

        public void Move() => player.Move();

        public void Draw()
        {
            player.Draw();
            CheckShieldStatus();

            if (shieldActive)
            {
                Raylib.DrawTexture(shieldTexture, X - 10, Y - 10, Color.White);
            }
        }

        public void Shoot(List<Bullet> bullets, Texture2D bulletTexture)
        {
            player.Shoot(bullets, bulletTexture);
        }
    }
}
