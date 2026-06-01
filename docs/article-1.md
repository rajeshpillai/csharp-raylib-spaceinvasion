# Building a Simple Space Invasion Game in C# with Raylib and Design Patterns on Ubuntu

In this tutorial, we'll build a Space Invasion game in C# using Raylib. Along the way we'll apply several design patterns to keep the code modular and flexible - the **Factory**, **Strategy**, and **Decorator** patterns.

> **NOTE:** This is an early draft of the tutorial, so you may find some redundancy in the code along with a few practices that could be improved. If you spot anything, please comment and share feedback - it will be folded into the next revision.

The graphics are intentionally kept simple, since visuals are not the focus of this tutorial.

## Prerequisites

- Basic knowledge of C#
- The [.NET SDK](https://dotnet.microsoft.com/download) installed
- Raylib - we'll add it as a NuGet package below. See [Raylib](https://www.raylib.com/) and the C# bindings: [raylib-cs](https://github.com/chrisdill/raylib-cs/tree/master)

## Game Overview

The player controls a spaceship that can move left or right and shoot bullets to destroy incoming enemies. The enemies move in different patterns, and the player can gain a temporary shield to block hits.

---

## Step 1: Setting Up the Project

Create a new console project:

```bash
dotnet new console -o SpaceInvasionGame
cd SpaceInvasionGame
```

Add Raylib as a dependency:

```bash
dotnet add package Raylib-cs
```

This updates your `.csproj` file with the package reference:

```xml
<ItemGroup>
  <PackageReference Include="Raylib-cs" Version="6.1.1" />
</ItemGroup>
```

Restore dependencies:

```bash
dotnet restore
```

---

## Step 2: Basic Game Window Setup

Create `Program.cs`. This file sets up the main game window and manages the game loop.

```csharp
using System;
using Raylib_cs;

namespace SpaceInvasionGame
{
    public class Program
    {
        private const int WindowWidth = 800;
        private const int WindowHeight = 600;

        public static void Main()
        {
            Raylib.InitWindow(WindowWidth, WindowHeight, "Space Invasion");
            Raylib.SetTargetFPS(60);

            while (!Raylib.WindowShouldClose())
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(Color.Black);
                Raylib.DrawText("Space Invasion Game", 10, 10, 20, Color.White);
                Raylib.EndDrawing();
            }

            Raylib.CloseWindow();
        }
    }
}
```

Run the game with `dotnet run` to confirm the window opens.

> **Assets:** Before the full game runs, create an `images/` folder in the project root containing `background.jpg`, `player.png`, `enemy.png`, `bullet.png`, and `shield.png`. Raylib loads these by relative path at startup, so they must be copied next to the executable (or set `<CopyToOutputDirectory>` in the `.csproj`).

---

## Step 3: Creating the Game Entities

We'll set up the core game elements: `Player`, `Enemy`, and `Bullet`. Shared behaviour lives in a common `GameEntity` base class.

### 3.1 The `GameEntity` Base Class

To avoid duplicating code, we define an abstract base class holding properties and methods common to all entities - position (`X`, `Y`), the texture, and drawing.

Create a folder called `Entity` to organize the files, then add `Entity/GameEntity.cs`:

```csharp
namespace SpaceInvasionGame.Entity
{
    using Raylib_cs;

    public abstract class GameEntity
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Texture2D Texture { get; protected set; }

        public GameEntity(int x, int y, Texture2D texture)
        {
            X = x;
            Y = y;
            Texture = texture;
        }

        // Abstract method for moving, to be implemented by derived classes
        public abstract void Move();

        // Draws the entity on the screen
        public virtual void Draw()
        {
            Raylib.DrawTexture(Texture, X, Y, Color.White);
        }
    }
}
```

> **Note on consistency:** In this draft, `Enemy` inherits from `GameEntity`, but `Player` and `Bullet` do not - they re-declare `X`, `Y`, and their texture themselves. A cleaner design would have all three derive from `GameEntity`. This is called out again below as an exercise.

### 3.2 The `IPlayer` Interface

The player has unique actions, such as shooting. We define an `IPlayer` interface so we can later modify the player's behaviour using the **Decorator Pattern** (for example, adding a shield) without changing the core `Player` class.

Create `Entity/IPlayer.cs`:

```csharp
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
```

### 3.3 The `Player` Class

Now we create the `Player` class that implements `IPlayer` and manages movement and shooting.

Create `Entity/Player.cs`:

```csharp
using Raylib_cs;
using System.Collections.Generic;

namespace SpaceInvasionGame.Entity
{
    public class Player : IPlayer
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        private int speed;
        private Texture2D texture;

        public Player(int x, int y, int speed, Texture2D texture)
        {
            X = x;
            Y = y;
            this.speed = speed;
            this.texture = texture;
        }

        public void Move()
        {
            if (Raylib.IsKeyDown(KeyboardKey.Left)) X -= speed;
            if (Raylib.IsKeyDown(KeyboardKey.Right)) X += speed;

            if (X < 0) X = 0;
            if (X > 736) X = 736;
        }

        public void Draw()
        {
            Raylib.DrawTexture(texture, X, Y, Color.White);
        }

        public void Shoot(List<Bullet> bullets, Texture2D bulletTexture)
        {
            bullets.Add(new Bullet(X + 28, Y, bulletTexture));
        }
    }
}
```

**Explanation**

- **`Move`** - handles left/right movement, clamped to the screen. The `736` upper bound is the window width (`800`) minus the player sprite width (`~64`), keeping the ship fully on screen.
- **`Shoot`** - adds a bullet to a list when fired. The `X + 28` offset centres the bullet over the ship. The list lets the game track and render every bullet in flight.

### 3.4 The `Bullet` Class

Bullets move upward and disappear when they leave the screen.

Create `Entity/Bullet.cs`:

```csharp
namespace SpaceInvasionGame.Entity
{
    using Raylib_cs;

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
}
```

**Explanation**

- **`Move`** - moves the bullet upward (decreasing `Y`).
- **`Draw`** - renders the bullet.

> **Exercise:** `Bullet` has the same shape as `GameEntity` (position + texture + draw). Could it derive from `GameEntity` instead of re-declaring those members? The only obstacle is that `GameEntity.Move()` is abstract and parameterless, which fits `Bullet` perfectly - so yes, this is a good refactor to try.

### 3.5 The `Enemy` Class

The `Enemy` is the third entity. It **inherits from `GameEntity`** and delegates its movement to a pluggable strategy object (defined in Step 5). It also handles collision detection against bullets.

Create `Entity/Enemy.cs`:

```csharp
namespace SpaceInvasionGame.Entity
{
    using Raylib_cs;

    public class Enemy : GameEntity
    {
        public int SpeedX { get; set; }
        public int SpeedY { get; set; }
        public bool MovingRight { get; set; } = true;
        private IMovementStrategy movementStrategy;

        public Enemy(int x, int y, int speedX, int speedY, Texture2D texture, IMovementStrategy strategy)
            : base(x, y, texture)
        {
            SpeedX = speedX;
            SpeedY = speedY;
            movementStrategy = strategy;
        }

        public override void Move()
        {
            movementStrategy.Move(this);
        }

        // Collision detection between this enemy and a bullet
        public bool Intersects(Bullet bullet)
        {
            Rectangle enemyRect = new Rectangle(X, Y, Texture.Width, Texture.Height);
            Rectangle bulletRect = new Rectangle(bullet.X, bullet.Y, bullet.Texture.Width, bullet.Texture.Height);
            return Raylib.CheckCollisionRecs(enemyRect, bulletRect);
        }
    }
}
```

**Explanation**

- **Constructor** - passes `x`, `y`, and `texture` up to the `GameEntity` base, and stores its speeds plus a movement strategy. `MovingRight` tracks the current horizontal direction.
- **`Move`** - instead of containing movement logic itself, the enemy hands `this` to its `IMovementStrategy`. Swapping the strategy changes how the enemy behaves - that's the Strategy Pattern in action (Step 5).
- **`Intersects`** - builds a rectangle for the enemy and for the bullet and uses Raylib's `CheckCollisionRecs` for axis-aligned bounding-box collision detection.

---

## Step 4: Using the Factory Pattern for Enemy Creation

The **Factory Pattern** lets us create objects without scattering construction details throughout the code. `EnemyFactory` centralizes enemy creation and makes it easy to adjust spawn behaviour.

Create `Entity/IEntityFactory.cs`:

```csharp
namespace SpaceInvasionGame.Entity
{
    public interface IEntityFactory
    {
        GameEntity Create();
    }
}
```

Create `Entity/EnemyFactory.cs`:

```csharp
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
```

**Explanation**

- **`EnemyFactory`** - spawns enemies at random horizontal positions (`0`-`736`) and near the top of the screen (`y` between `50` and `150`), each with a `ZigZagMovement` strategy. Because creation lives in one place, you could later return different enemy types or strategies without touching the game loop.

---

## Step 5: Adding Movement Patterns (Strategy Pattern)

The **Strategy Pattern** lets us give enemies interchangeable movement behaviours, so new patterns can be added without modifying the `Enemy` class.

Create `Entity/IMovementStrategy.cs`:

```csharp
namespace SpaceInvasionGame.Entity
{
    public interface IMovementStrategy
    {
        void Move(Enemy enemy);
    }
}
```

Create `Entity/ZigZagMovement.cs`:

```csharp
namespace SpaceInvasionGame.Entity
{
    public class ZigZagMovement : IMovementStrategy
    {
        public void Move(Enemy enemy)
        {
            enemy.X += enemy.MovingRight ? enemy.SpeedX : -enemy.SpeedX;
            if (enemy.X <= 0 || enemy.X >= 736)
            {
                enemy.MovingRight = !enemy.MovingRight;
                enemy.Y += enemy.SpeedY;
            }
        }
    }
}
```

**Explanation**

- **`ZigZagMovement`** - moves the enemy horizontally; on reaching either screen edge it flips direction and drops down by `SpeedY`, producing the classic zig-zag descent. To add a new pattern (straight dive, sine wave, etc.), implement `IMovementStrategy` and pass it to the `Enemy` constructor.

---

## Step 6: Adding a Shield (Decorator Pattern)

The **Decorator Pattern** lets us add functionality to an object dynamically. We use it to wrap the player with a temporary shield while keeping the `IPlayer` contract intact.

Create `Entity/ShieldDecorator.cs`:

```csharp
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
```

**Explanation**

- **`ShieldDecorator`** implements `IPlayer` and wraps another `IPlayer`. Every method delegates to the wrapped player, so it's a drop-in replacement.
- **`ActivateShield`** - turns the shield on and records the time.
- **`CheckShieldStatus`** - automatically deactivates the shield once `shieldDuration` seconds have elapsed.
- **`Draw`** - draws the underlying player first, then overlays the shield texture when active.

> **Heads-up - the shield is never triggered yet.** `ActivateShield()` is defined but nothing in `Program.cs` calls it, so the shield never appears at runtime. To make it work, hold a reference to the decorator and bind a key, for example:
>
> ```csharp
> // store the decorator so we can call its extra method
> ShieldDecorator shieldedPlayer = new ShieldDecorator(player, shieldTexture);
> player = shieldedPlayer;
> // ...later, inside HandleEvents():
> if (Raylib.IsKeyPressed(KeyboardKey.S)) shieldedPlayer.ActivateShield();
> ```
>
> This is a good exercise to wire up. Note also that the shield is purely visual right now - it does not yet block enemy hits.

---

## Final Step: Integrating Everything in `Program.cs`

With all the components in place, we tie them together in `Program.cs`.

```csharp
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

            player = new Player(370, 480, 3, playerTexture);
            // Add shield to player with Decorator pattern
            player = new ShieldDecorator(player, shieldTexture);

            enemies = new List<Enemy>();
            bullets = new List<Bullet>();
            random = new Random();
            score = 0;
            gameOver = false;

            IEntityFactory enemyFactory = new EnemyFactory(enemyTexture, random);
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
            player.Move();

            if (Raylib.IsKeyPressed(KeyboardKey.Space) && !gameOver)
            {
                bullets.Add(new Bullet(player.X + 28, player.Y, bulletTexture));
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

            foreach (Enemy enemy in enemies)
            {
                enemy.Move();
                if (enemy.Y > 440)
                {
                    gameOver = true;
                }
            }

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                for (int j = bullets.Count - 1; j >= 0; j--)
                {
                    if (enemies[i].Intersects(bullets[j]))
                    {
                        enemies.RemoveAt(i);
                        bullets.RemoveAt(j);
                        score++;
                        enemies.Add(new Enemy(random.Next(0, 736), random.Next(50, 150), 2, 40, enemyTexture, new ZigZagMovement()));

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
```

**How the loop works**

- **`HandleEvents`** - moves the player and fires a bullet on the space bar.
- **`UpdateGame`** - advances bullets and enemies, removes off-screen bullets, ends the game if an enemy descends past `y = 440`, and checks every bullet against every enemy. On a hit it removes both, increments the score, and spawns a replacement enemy so the wave never empties. (Iterating the lists backwards lets us remove items safely mid-loop.)
- **`Render`** - draws the background, player, enemies, bullets, the score, and the "GAME OVER" text.

> **Pattern consistency note:** when an enemy is destroyed, the replacement is created with `new Enemy(...)` directly instead of going through `enemyFactory.Create()`. Routing this through the factory would remove the duplicated construction logic - another small refactor worth doing.

---

## Conclusion

This concludes the tutorial - you now have a fully functioning simple version of Space Invasion in C#. By applying design patterns, the code stays flexible, organized, and easy to extend with new features such as different enemy types, movement patterns, and player abilities.

**Suggested next steps / exercises**

1. Make `Player` and `Bullet` derive from `GameEntity` to remove duplicated `X`/`Y`/texture code.
2. Wire a key to `ShieldDecorator.ActivateShield()` and make the shield actually absorb a hit.
3. Add a second `IMovementStrategy` (e.g. straight dive or sine wave) and have the factory pick one at random.
4. Spawn replacement enemies through `EnemyFactory.Create()` instead of `new Enemy(...)`.

The experimental code is at <https://github.com/rajeshpillai/csharp-raylib-spaceinvasion/tree/feat-design-patterns-1>.
