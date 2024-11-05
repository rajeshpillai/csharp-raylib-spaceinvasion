namespace SpaceInvasionGame.Entity
{
    using Raylib_cs;
    using System;

    public class EnemyFactory : IEntityFactory
    {
        private Texture2D enemyTexture;
        private Random random;

        public EnemyFactory(Texture2D texture, Random random)
        {
            enemyTexture = texture;
            this.random = random;
        }

        public GameEntity Create()
        {
            return new Enemy(random.Next(0, 736), random.Next(50, 150), 2, 40, enemyTexture, new ZigZagMovement());
        }
    }
}
