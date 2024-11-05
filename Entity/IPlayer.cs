using Raylib_cs;
using System.Collections.Generic;

namespace SpaceInvasionGame.Entity
{
    public interface IPlayer
    {
        int X { get; }
        int Y { get; }
        void Move();
        void Draw();
        void Shoot(List<Bullet> bullets, Texture2D bulletTexture);
    }
}
