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
            return new Enemy(
                random.Next(0, 736),
                random.Next(50, 150),
                2,
                40,
                enemyTexture,
                CreateRandomStrategy());
        }

        // The factory decides which movement behaviour each enemy gets.
        private IMovementStrategy CreateRandomStrategy()
        {
            switch (random.Next(3))
            {
                case 0: return new StraightDiveMovement();
                case 1: return new SineWaveMovement();
                default: return new ZigZagMovement();
            }
        }
    }
}
