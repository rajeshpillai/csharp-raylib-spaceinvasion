# Space Invasion Game

This project is a Space Invasion game built using C# and Raylib. The game demonstrates various design patterns such as **Factory**, **Strategy**, and **Decorator** to enhance code modularity, flexibility, and readability. 

## Game Overview

In Space Invasion, the player controls a spaceship to shoot incoming enemies. Each enemy moves in a particular pattern, and the player can collect a power-up that provides a temporary shield around their ship. The game also keeps score based on the number of enemies destroyed.

## Code Structure

The code is organized into separate files for each entity and pattern implementation:


## Design Patterns Used

### 1. Factory Pattern

**Purpose**: The Factory pattern is used to encapsulate the creation of game entities (like `Enemy` and `Bullet`). This approach allows the main game logic to request new entities without needing to know the specifics of how they're created.

**Implementation**:
- `IEntityFactory` interface defines a `Create` method that returns a `GameEntity`.
- `EnemyFactory` and `BulletFactory` implement `IEntityFactory` and create instances of `Enemy` and `Bullet` respectively.
  
**Usage**:
```csharp
IEntityFactory enemyFactory = new EnemyFactory(enemyTexture, random);
enemies.Add((Enemy)enemyFactory.Create());
```

When Applied: This pattern is applied whenever a new game entity (like an Enemy) needs to be created. It allows for easy extension to add new types of enemies without changing the core game logic.

2. Strategy Pattern

Purpose: The Strategy pattern defines different movement behaviors for enemies, enabling them to move in varied ways (e.g., zigzag, straight). This helps decouple movement logic from the enemy class and allows new behaviors to be added easily.

Implementation:
  - IMovementStrategy interface defines a Move method.
  - StraightMovement and ZigZagMovement classes implement IMovementStrategy and provide specific movement behaviors.
  - Enemy class accepts an IMovementStrategy instance and delegates its Move method to the strategy.

```dotnetcli 
IMovementStrategy zigzagStrategy = new ZigZagMovement();
enemies.Add(new Enemy(randomX, randomY, speedX, speedY, enemyTexture, zigzagStrategy));
```

When Applied: This pattern is applied when creating an Enemy. By injecting different movement strategies, we can change how each enemy behaves without altering the Enemy class itself.

3. Decorator Pattern

Purpose: The Decorator pattern adds extra functionality to the player (like a shield) without altering the Player class. This allows the player to gain temporary power-ups that modify behavior.

Implementation:

    IPlayer interface defines basic player actions.
    Player class implements IPlayer, providing default player behavior.
    ShieldDecorator wraps a Player instance, adding a shield that activates for a limited time.

Usage:

```dotnetcli IPlayer player = new Player(initialX, initialY, playerSpeed, playerTexture);
player = new ShieldDecorator(player, shieldTexture);
```

When Applied: This pattern is applied when the player gains a power-up. By using the ShieldDecorator, we can add shield functionality dynamically, allowing the player to become temporarily invincible.

## Detailed Code Explanation
Main Game Logic (Program.cs)

    Initialization: The game window is set up, and textures are loaded. The player and enemies are created using the Factory pattern.
    Event Handling: Player input is managed to control movement and shooting.
    Game Update: The player, bullets, and enemies are updated. Bullet-enemy collisions are checked, and the score is increased when an enemy is hit.
    Render: All entities are drawn to the screen, and the score and "Game Over" message are displayed if applicable.

Collision Detection in Enemy

The Enemy class includes an Intersects method to check for collisions with bullets:

```dotnetcli
public bool Intersects(Bullet bullet)
{
    Rectangle enemyRect = new Rectangle(X, Y, Texture.width, Texture.height);
    Rectangle bulletRect = new Rectangle(bullet.X, bullet.Y, bullet.Texture.width, bullet.Texture.height);
    return Raylib.CheckCollisionRecs(enemyRect, bulletRect);
}
```

This method creates rectangles for both the enemy and the bullet based on their positions and dimensions. Raylib.CheckCollisionRecs checks if these rectangles intersect, indicating a collision.

## Shield Power-Up with ShieldDecorator

The ShieldDecorator wraps the Player object, adding a temporary shield that can be activated for a limited duration:

```dotnetcli 
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
```

The shield is activated by calling ActivateShield, and CheckShieldStatus automatically deactivates it after the specified duration.
How to Run

    Ensure you have .NET and Raylib installed.
    Clone the repository and navigate to the project directory.
    Run the following commands:

```dotnetcli
dotnet restore
dotnet build
dotnet run
```

# Conclusion

This Space Invasion game showcases how design patterns can improve the modularity, flexibility, and extensibility of game code. By using Factory, Strategy, and Decorator patterns, we enable dynamic entity creation, customizable behaviors, and the ability to add temporary power-ups easily.


