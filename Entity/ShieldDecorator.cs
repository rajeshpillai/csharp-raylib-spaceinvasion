using Raylib_cs;
using System.Collections.Generic;

namespace SpaceInvasionGame.Entity
{
    public class ShieldDecorator : IPlayer
    {
        private IPlayer player;
        private Texture2D shieldTexture;
        private bool shieldActive;
        private float shieldDuration;
        private double shieldActivatedTime;

        public int X => player.X;
        public int Y => player.Y;

        // Lets the game logic ask whether the player is currently protected.
        public bool IsShieldActive
        {
            get
            {
                CheckShieldStatus();
                return shieldActive;
            }
        }

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
            shieldActivatedTime = Raylib.GetTime();
        }

        private void CheckShieldStatus()
        {
            if (shieldActive && (Raylib.GetTime() - shieldActivatedTime) >= shieldDuration)
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
