namespace SpaceInvasionGame.Entity
{
    using Raylib_cs;
    using System;

    // Sways left and right in a sine wave while slowly descending.
    // Keeps its own per-enemy state (one strategy instance is created per enemy).
    public class SineWaveMovement : IMovementStrategy
    {
        private double startTime = -1;
        private int baseX;

        public void Move(Enemy enemy)
        {
            if (startTime < 0)
            {
                startTime = Raylib.GetTime();
                baseX = enemy.X;
            }

            double t = Raylib.GetTime() - startTime;
            int x = baseX + (int)(Math.Sin(t * 3) * 60);

            if (x < 0) x = 0;
            if (x > 736) x = 736;

            enemy.X = x;
            enemy.Y += 1;
        }
    }
}
