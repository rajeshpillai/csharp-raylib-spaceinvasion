# Space Invasion Game

## Overview

The Space Invasion Game is a simple 2D shooter game built in C# using the Raylib library. In this game, you control a player spaceship that must shoot down enemies while avoiding their approach. The game runs in a graphical window and features enemy movement, player movement, bullet shooting, and score tracking. If the enemies get too close, the game ends.

## Features

Player Movement: Control the spaceship using the Left and Right arrow keys.

Enemy Movement: Enemies move across the screen and descend after reaching the edge.

Shooting Bullets: Shoot bullets to destroy enemies using the Space key.

Score Tracking: The game keeps track of the player's score.

Game Over: The game ends when enemies reach the bottom of the screen.

## Controls

Left Arrow Key: Move the player to the left.

Right Arrow Key: Move the player to the right.

Space Key: Shoot bullets.

## Prerequisites

To run this game, you need:

.NET SDK (6.0 or higher)

Raylib-cs package

## Installation

Clone the Repository:

git clone <repository-url>
cd SpaceInvasionGame

Install Dependencies:

Install the Raylib-cs NuGet package:

dotnet add package Raylib-cs 

Build the Project:

dotnet build

Running the Game

After building the project, you can run the game with:

dotnet run

Code Structure

SpaceInvasionGame: The main game class that initializes the window, handles player input, updates game states, and renders graphics.

Enemy: Represents an enemy in the game. Handles movement and collision detection.

Bullet: Represents a bullet fired by the player. Handles movement and rendering.

Gameplay

The player controls a triangular spaceship at the bottom of the screen, which can move left and right. Enemies move across the screen and descend after reaching the edges. The player can shoot bullets to destroy enemies, and each destroyed enemy increases the score. If an enemy reaches the bottom of the screen, the game ends.

License

This game is open-source and available under the MIT License.

Acknowledgements

Built with Raylib and Raylib-cs.

Inspired by classic arcade space shooter games.