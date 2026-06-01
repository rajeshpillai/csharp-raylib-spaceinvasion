# Space Invasion in C# with Raylib - Part 2: Refactoring and Enhancements

This is **Part 2** of the Space Invasion tutorial. In [Part 1](article-1.md) we built a working game and introduced the **Factory**, **Strategy**, and **Decorator** patterns. We also left a handful of deliberate loose ends and called them out as exercises.

In this part we close those gaps and add a few enhancements that make the patterns actually earn their keep:

1. **Refactor** - make every entity derive from `GameEntity`, removing duplicated code.
2. **Strategy** - add two new movement patterns and let the factory choose one at random.
3. **Decorator** - wire up the shield so it can be activated *and* genuinely protects the player.
4. **Factory** - spawn replacement enemies through the factory instead of `new Enemy(...)`.
5. **Quality of life** - fire through the player's own `Shoot` method, add a restart, and show on-screen controls.

If you followed Part 1, you can keep editing the same project.

---

## 1. One Base Class for Every Entity

In Part 1 only `Enemy` inherited from `GameEntity`; `Player` and `Bullet` re-declared `X`, `Y`, and their texture themselves. That duplication is exactly what a base class is meant to remove.

### `Bullet` derives from `GameEntity`

`Bullet` already had the same shape as `GameEntity` (position + texture + draw). The only thing unique to it is upward movement, so it just overrides `Move()`:

```csharp
namespace SpaceInvasionGame.Entity
{
    using Raylib_cs;

    public class Bullet : GameEntity
    {
        private int speedY = 6;

        public Bullet(int x, int y, Texture2D texture)
            : base(x, y, texture)
        {
        }

        // Bullets travel straight up the screen.
        public override void Move()
        {
            Y -= speedY;
        }
    }
}
```

The `X`, `Y`, `Texture`, and `Draw()` members now all come from `GameEntity`. Note that `Enemy.Intersects` still compiles unchanged - it reads `bullet.X`, `bullet.Y`, and `bullet.Texture`, which are now the inherited properties.

### `Player` derives from `GameEntity` (and still implements `IPlayer`)

A class can extend a base class *and* implement an interface at the same time. `Player` keeps its `IPlayer` contract (needed by the Decorator) while inheriting the shared members:

```csharp
using Raylib_cs;
using System.Collections.Generic;

namespace SpaceInvasionGame.Entity
{
    public class Player : GameEntity, IPlayer
    {
        private int speed;

        public Player(int x, int y, int speed, Texture2D texture)
            : base(x, y, texture)
        {
            this.speed = speed;
        }

        public override void Move()
        {
            if (Raylib.IsKeyDown(KeyboardKey.Left)) X -= speed;
            if (Raylib.IsKeyDown(KeyboardKey.Right)) X += speed;

            if (X < 0) X = 0;
            if (X > 736) X = 736;
        }

        public void Shoot(List<Bullet> bullets, Texture2D bulletTexture)
        {
            bullets.Add(new Bullet(X + 28, Y, bulletTexture));
        }
    }
}
```

**Why this works without extra code:**

- `IPlayer` requires `int X { get; }` and `int Y { get; }` - satisfied by the public properties on `GameEntity`.
- `IPlayer` requires `void Draw()` - satisfied by the inherited virtual `GameEntity.Draw()`.
- `Move()` is now an `override` of the abstract base method instead of a brand-new method.

The duplicated fields are gone, and `Player` is shorter for it.

### A shared `Bounds` for collisions

Now that every entity shares a base class, we can give them all a collision rectangle in one place. Add a `Bounds` property to `GameEntity`:

```csharp
// Axis-aligned bounding box used for collision checks.
public Rectangle Bounds => new Rectangle(X, Y, Texture.Width, Texture.Height);
```

With that, `Enemy.Intersects` collapses to a one-liner and - because it now takes a `GameEntity` - works against bullets *and* the player:

```csharp
// Collision detection between this enemy and any other entity.
public bool Intersects(GameEntity other)
{
    return Raylib.CheckCollisionRecs(Bounds, other.Bounds);
}
```

This single change is what lets us fix the "game ends too early" bug in [Part 1](article-1.md): instead of comparing an enemy's `Y` against a fixed line, we can ask whether it truly overlaps the player. More on that in the `Program.cs` walkthrough below.

---

## 2. Strategy Pattern: More Ways to Move

In Part 1 every enemy used `ZigZagMovement`. The whole point of the Strategy Pattern is that adding a new behaviour shouldn't touch the `Enemy` class at all - we just write a new `IMovementStrategy`.

### `StraightDiveMovement`

A no-frills vertical descent. It reuses `SpeedX` as the per-frame step (the zig-zag's `SpeedY` of `40` is a per-edge drop, far too large to apply every frame):

```csharp
namespace SpaceInvasionGame.Entity
{
    // Descends straight down at a steady pace, no horizontal movement.
    public class StraightDiveMovement : IMovementStrategy
    {
        public void Move(Enemy enemy)
        {
            enemy.Y += enemy.SpeedX;
        }
    }
}
```

### `SineWaveMovement`

This one sways side to side while drifting down. It needs a little per-enemy state (when it started and its original `X`), which is fine because **a fresh strategy instance is created for each enemy** - they never share state:

```csharp
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
```

`Raylib.GetTime()` returns the seconds elapsed since the window opened - handy for time-based motion that doesn't depend on frame count.

### The factory chooses the behaviour

Because the factory is the single place enemies are created, it's the natural home for "what kind of enemy is this?" logic. We give it a small helper that picks a strategy at random:

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
```

Every wave now mixes divers, weavers, and zig-zaggers - and `Enemy` itself never changed.

---

## 3. Decorator Pattern: A Shield That Actually Works

In Part 1 the `ShieldDecorator` had an `ActivateShield()` method that nothing ever called, and even when drawn the shield didn't block anything. Let's fix both.

We make two changes to the decorator:

1. Swap `DateTime.Now` for `Raylib.GetTime()` so the timer uses the same clock as the rest of the game.
2. Expose an `IsShieldActive` property so the game loop can ask whether the player is currently protected.

```csharp
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
```

The decorator still exposes only the `IPlayer` interface to most of the game. The extra capability (`ActivateShield`, `IsShieldActive`) is used by the small amount of code that knows it's holding a *shielded* player - which we set up next.

---

## 4. Tying It Together in `Program.cs`

The game shell changes in several small ways. Let's walk through each before the full listing.

### Keep a typed reference to the shield and the factory

Most of the game talks to `IPlayer`, but to call `ActivateShield()` and read `IsShieldActive` we keep a `ShieldDecorator` reference too. We also keep the base `Player` (so we can reset its position on restart) and the factory (so respawns go through it):

```csharp
private IPlayer player;
private Player basePlayer;
private ShieldDecorator shield;
private IEntityFactory enemyFactory;
```

Setup wires the decorator around the player:

```csharp
basePlayer = new Player(370, 480, 3, playerTexture);
shield = new ShieldDecorator(basePlayer, shieldTexture);
player = shield;

enemyFactory = new EnemyFactory(enemyTexture, random);
ResetGame();
```

### A reusable `ResetGame()` for start *and* restart

Instead of populating the field in the constructor only, we move it into a method so a game-over can call it again:

```csharp
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
```

### Input: shoot through the player, plus shield and restart keys

Two fixes here. First, firing now goes through `player.Shoot(...)` - the method we defined back in Part 1 but never used (the old loop did `new Bullet(...)` directly). Routing through `IPlayer` means the decorator chain is respected. Second, we add **S** for the shield and **R** to restart after a loss:

```csharp
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
```

### Update: end the game on a *real* collision, respawns use the factory

This is the most important fix in Part 2. In Part 1 the game ended the moment any enemy crossed a fixed `y` line (`enemy.Y > 440`) - **regardless of its horizontal position**. That meant an enemy descending on the far side of the screen, nowhere near your ship, would still end the game. It looked like the game ended "before the enemy collided."

The fix is to end the game only when an enemy's rectangle actually overlaps the player's rectangle. We already have a reusable `Intersects` helper and a `Bounds` property (added to `GameEntity` back in §1), so the check reads naturally. Enemies that miss the player and fall off the bottom are recycled through the factory:

```csharp
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
```

> Both enemy loops iterate **backwards** so we can remove items mid-loop without skipping the next element or going out of bounds.

We collide against `basePlayer` (the concrete `Player`) rather than the `IPlayer` field because collision needs the player's `Bounds`, which comes from `GameEntity`. The shield wrapper only exposes the `IPlayer` surface.

### The complete `Program.cs`

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
```

---

## What We Changed, and Why It Matters

| Area | Part 1 | Part 2 |
| --- | --- | --- |
| Entities | Only `Enemy : GameEntity` | `Player`, `Bullet`, `Enemy` all share `GameEntity` |
| Movement | One hard-coded `ZigZagMovement` | Three strategies, picked at random by the factory |
| Shield | Method existed but was never called or effective | Activated with **S**, absorbs colliding enemies for 5s |
| End condition | Fixed `y` line, ignored horizontal position | True rectangle collision between enemy and player |
| Respawns | `new Enemy(...)` inline | Always `enemyFactory.Create()` |
| Shooting | `new Bullet(...)` directly in the loop | Through `player.Shoot(...)` (respects the decorator) |
| Game over | Dead end | **R** restarts via shared `ResetGame()` |

The payoff is that each pattern now does something concrete: the **Factory** is the one place enemy creation lives, the **Strategy** lets us add behaviours without editing `Enemy`, and the **Decorator** adds a real, optional capability to the player without the player class knowing about it.

## Ideas for Part 3

- **Lives instead of instant game over** - give the player three hits before the end screen.
- **A `StrategyFactory`** so movement selection is configurable (e.g. harder waves favour divers).
- **Sound effects** with Raylib's audio module on shoot, hit, and shield.
- **An Observer/event system** so score, sound, and UI react to "enemy destroyed" without the update loop wiring them together.
- **Frame-rate-independent movement** by multiplying speeds by `Raylib.GetFrameTime()`.

The code for this part lives in the project repository: <https://github.com/rajeshpillai/csharp-raylib-spaceinvasion>.
