using System;
using System.Collections.Generic;
using Raylib_cs;
using SpaceInvasionGame.Entity;

namespace SpaceInvasionGame {
    public class SpaceInvasionGame
    {
        private const int WindowWidth = 800;
        private const int WindowHeight = 600;
        private Player player;
        private List<Enemy> enemies;
        private List<Bullet> bullets;
        private Random random;
        private int score;
        private bool gameOver;

        // Load textures
        private Texture2D background;
        private Texture2D enemyTexture;
        private Texture2D bulletTexture;

        public SpaceInvasionGame()
        {
            Raylib.InitWindow(WindowWidth, WindowHeight, "Space Invasion");
            Raylib.SetTargetFPS(60);

            // Load assets
            background = Raylib.LoadTexture("images/background.jpg");
            Texture2D playerTexture = Raylib.LoadTexture("images/player.png");
            enemyTexture = Raylib.LoadTexture("images/enemy.png");
            bulletTexture = Raylib.LoadTexture("images/bullet.png");

            player = new Player(370, 480, 3, playerTexture);
            enemies = new List<Enemy>();
            bullets = new List<Bullet>();
            random = new Random();
            score = 0;
            gameOver = false;

            for (int i = 0; i < 6; i++)
            {
                enemies.Add(new Enemy(random.Next(0, 736), random.Next(50, 150), 2, 40, enemyTexture));
            }
        }

        public void Run()
        {
            while (!Raylib.WindowShouldClose())
            {
                HandleEvents();
                UpdateGame();
                Render();
            }

            // Unload textures
            Raylib.UnloadTexture(background);
            Raylib.UnloadTexture(player.Texture);
            Raylib.UnloadTexture(enemyTexture);
            Raylib.UnloadTexture(bulletTexture);

            Raylib.CloseWindow();
        }

        private void HandleEvents()
        {
            player.HandleMovement();
            
            if (Raylib.IsKeyPressed(KeyboardKey.Space) && !gameOver)
            {
                bullets.Add(new Bullet(player.X + 28, player.Y, bulletTexture));
            }
        }

        private void UpdateGame()
        {
            if (gameOver) return;

            player.UpdatePosition();

            // Bullet movement
            foreach (Bullet bullet in bullets)
            {
                bullet.Move();
            }
            bullets.RemoveAll(b => b.Y < 0);

            // Enemy movement
            foreach (Enemy enemy in enemies)
            {
                enemy.Move();
                if (enemy.Y > 440)
                {
                    gameOver = true;
                }
            }

            // Bullet-enemy collision detection
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                for (int j = bullets.Count - 1; j >= 0; j--)
                {
                    if (enemies[i].Intersects(bullets[j]))
                    {
                        enemies.RemoveAt(i);
                        bullets.RemoveAt(j);
                        score++;
                        enemies.Add(new Enemy(random.Next(0, 736), random.Next(50, 150), 2, 40, enemyTexture));
                        break;
                    }
                }
            }
        }

        private void Render()
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            Raylib.DrawTexture(background, 0, 0, Color.White);
            player.Draw();

            foreach (Enemy enemy in enemies)
            {
                enemy.Draw();
            }

            foreach (Bullet bullet in bullets)
            {
                bullet.Draw();
            }

            Raylib.DrawText($"Score: {score}", 10, 10, 20, Color.White);

            if (gameOver)
            {
                Raylib.DrawText("GAME OVER", 250, 250, 32, Color.White);
            }

            Raylib.EndDrawing();
        }

        public static void Main()
        {
            var game = new SpaceInvasionGame();
            game.Run();
        }
    }
}