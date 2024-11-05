using System;
using System.Collections.Generic;
using Raylib_cs;

public class SpaceInvasionGame
{
    private const int WindowWidth = 800;
    private const int WindowHeight = 600;
    private int playerX = 370;
    private int playerY = 480;
    private int playerSpeed = 3;
    private bool moveLeft, moveRight;
    private List<Enemy> enemies;
    private List<Bullet> bullets;
    private Random random;
    private int score;
    private bool gameOver;

    // Load textures
    private Texture2D background;
    private Texture2D playerTexture;
    private Texture2D enemyTexture;
    private Texture2D bulletTexture;

    public SpaceInvasionGame()
    {
        Raylib.InitWindow(WindowWidth, WindowHeight, "Space Invasion");
        Raylib.SetTargetFPS(60);

        // Load assets
        background = Raylib.LoadTexture("images/background.jpg");
        playerTexture = Raylib.LoadTexture("images/player.png");
        enemyTexture = Raylib.LoadTexture("images/enemy.png");
        bulletTexture = Raylib.LoadTexture("images/bullet.png");

        enemies = new List<Enemy>();
        bullets = new List<Bullet>();
        random = new Random();
        score = 0;
        gameOver = false;

        for (int i = 0; i < 6; i++)
        {
            enemies.Add(new Enemy(random.Next(0, 736), random.Next(50, 150), 4, 40, enemyTexture));
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
        Raylib.UnloadTexture(playerTexture);
        Raylib.UnloadTexture(enemyTexture);
        Raylib.UnloadTexture(bulletTexture);

        Raylib.CloseWindow();
    }

    private void HandleEvents()
    {
        if (Raylib.IsKeyDown(KeyboardKey.Left))
            moveLeft = true;
        else
            moveLeft = false;

        if (Raylib.IsKeyDown(KeyboardKey.Right))
            moveRight = true;
        else
            moveRight = false;

        if (Raylib.IsKeyPressed(KeyboardKey.Space) && !gameOver)
        {
            bullets.Add(new Bullet(playerX + 28, playerY, bulletTexture));
        }
    }

    private void UpdateGame()
    {
        if (gameOver)
            return;

        // Player movement
        if (moveLeft)
            playerX -= playerSpeed;
        if (moveRight)
            playerX += playerSpeed;

        if (playerX < 0)
            playerX = 0;
        if (playerX > 736)
            playerX = 736;

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
                    enemies.Add(new Enemy(random.Next(0, 736), random.Next(50, 150), 4, 40, enemyTexture));
                    break;
                }
            }
        }
    }

    private void Render()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.Black);

        // Draw background
        Raylib.DrawTexture(background, 0, 0, Color.White);

        // Draw player
        Raylib.DrawTexture(playerTexture, playerX, playerY, Color.White);

        // Draw enemies
        foreach (Enemy enemy in enemies)
        {
            enemy.Draw();
        }

        // Draw bullets
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

public class Enemy
{
    public int X { get; set; }
    public int Y { get; set; }
    private int speedX;
    private int speedY;
    private bool movingRight = true;
    private Texture2D texture;

    public Enemy(int x, int y, int speedX, int speedY, Texture2D texture)
    {
        X = x;
        Y = y;
        this.speedX = speedX;
        this.speedY = speedY;
        this.texture = texture;
    }

    public void Move()
    {
        if (movingRight)
            X += speedX;
        else
            X -= speedX;

        if (X <= 0 || X >= 736)
        {
            movingRight = !movingRight;
            Y += speedY;
        }
    }

    public void Draw()
    {
        Raylib.DrawTexture(texture, X, Y, Color.White);
    }

    public bool Intersects(Bullet bullet)
    {
        Rectangle enemyRect = new Rectangle(X, Y, texture.Width, texture.Height);
        Rectangle bulletRect = new Rectangle(bullet.X, bullet.Y, bullet.Texture.Width, bullet.Texture.Height);
        return Raylib.CheckCollisionRecs(enemyRect, bulletRect);
    }
}

public class Bullet
{
    public int X { get; private set; }
    public int Y { get; private set; }
    private int speedY = 6;
    public Texture2D Texture { get; private set; }

    public Bullet(int x, int y, Texture2D texture)
    {
        X = x;
        Y = y;
        Texture = texture;
    }

    public void Move()
    {
        Y -= speedY;
    }

    public void Draw()
    {
        Raylib.DrawTexture(Texture, X, Y, Color.White);
    }
}
