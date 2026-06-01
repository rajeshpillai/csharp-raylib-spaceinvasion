using System;
using System.Collections.Generic;
using Raylib_cs;
using SpaceInvasionGame.Entity;

namespace SpaceInvasionGame {
    public class SpaceInvasionGame
    {
        private const int WindowWidth = 800;
        private const int WindowHeight = 600;

        private IPlayer player;
        private Player basePlayer;
        private ShieldDecorator shield;
        private IEntityFactory enemyFactory;
        private List<Enemy> enemies;
        private List<Bullet> bullets;
        private Random random;
        private int score;
        private bool gameOver;

        // Load textures
        private Texture2D background;
        private Texture2D enemyTexture;
        private Texture2D bulletTexture;
        private Texture2D shieldTexture;

        public SpaceInvasionGame()
        {
            Raylib.InitWindow(WindowWidth, WindowHeight, "Space Invasion");
            Raylib.SetTargetFPS(60);

            // Load assets
            background = Raylib.LoadTexture("images/background.jpg");
            Texture2D playerTexture = Raylib.LoadTexture("images/player.png");
            enemyTexture = Raylib.LoadTexture("images/enemy.png");
            bulletTexture = Raylib.LoadTexture("images/bullet.png");
            shieldTexture = Raylib.LoadTexture("images/shield.png");

            basePlayer = new Player(370, 480, 3, playerTexture);
            // Wrap the player with the shield decorator and keep a typed reference
            // so we can activate the shield and query its state.
            shield = new ShieldDecorator(basePlayer, shieldTexture);
            player = shield;

            enemies = new List<Enemy>();
            bullets = new List<Bullet>();
            random = new Random();
            enemyFactory = new EnemyFactory(enemyTexture, random);

            ResetGame();
        }

        // (Re)initialise the playfield. Used at startup and on restart.
        private void ResetGame()
        {
            score = 0;
            gameOver = false;
            bullets.Clear();
            enemies.Clear();
            basePlayer.X = 370;

            for (int i = 0; i < 6; i++)
            {
                enemies.Add((Enemy)enemyFactory.Create());
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

            Raylib.UnloadTexture(background);
            Raylib.UnloadTexture(enemyTexture);
            Raylib.UnloadTexture(bulletTexture);
            Raylib.UnloadTexture(shieldTexture);
            Raylib.CloseWindow();
        }

        private void HandleEvents()
        {
            if (gameOver)
            {
                if (Raylib.IsKeyPressed(KeyboardKey.R)) ResetGame();
                return;
            }

            player.Move();

            if (Raylib.IsKeyPressed(KeyboardKey.Space))
            {
                player.Shoot(bullets, bulletTexture);
            }

            if (Raylib.IsKeyPressed(KeyboardKey.S))
            {
                shield.ActivateShield();
            }
        }

        private void UpdateGame()
        {
            if (gameOver) return;

            foreach (Bullet bullet in bullets)
            {
                bullet.Move();
            }
            bullets.RemoveAll(b => b.Y < 0);

            // Move enemies, then check whether any actually collided with the player.
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                enemies[i].Move();

                if (enemies[i].Intersects(basePlayer))
                {
                    if (shield.IsShieldActive)
                    {
                        // The shield absorbs the enemy instead of ending the game.
                        enemies.RemoveAt(i);
                        enemies.Add((Enemy)enemyFactory.Create());
                    }
                    else
                    {
                        gameOver = true;
                    }
                }
                else if (enemies[i].Y > WindowHeight)
                {
                    // Enemy slipped past the bottom of the screen, recycle it.
                    enemies.RemoveAt(i);
                    enemies.Add((Enemy)enemyFactory.Create());
                }
            }

            // Bullet/enemy collisions.
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                for (int j = bullets.Count - 1; j >= 0; j--)
                {
                    if (enemies[i].Intersects(bullets[j]))
                    {
                        enemies.RemoveAt(i);
                        bullets.RemoveAt(j);
                        score++;
                        enemies.Add((Enemy)enemyFactory.Create());
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
            Raylib.DrawText("Move: <-/->   Shoot: Space   Shield: S", 10, 35, 16, Color.Gray);

            if (shield.IsShieldActive)
            {
                Raylib.DrawText("SHIELD ACTIVE", 10, 55, 16, Color.SkyBlue);
            }

            if (gameOver)
            {
                Raylib.DrawText("GAME OVER", 250, 250, 32, Color.White);
                Raylib.DrawText("Press R to restart", 280, 300, 20, Color.White);
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
