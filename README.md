# Unity FPS Maze Game
![GitHub code size in bytes](https://img.shields.io/github/languages/code-size/christopher-boustros/Unity-FPS-Maze-Game)

A 3D first-person shooter game made with Unity in which the player must traverse and destroy a randomly-generated maze. This game was made as part of a course assignment for COMP 521 Modern Computer Games in fall 2020 at McGill University. The goal of the assignment was to implement maze generation algorithms. 

The game generates a random unicursal path leading to the edge of the canyon and a random perfect 5x5 maze that spans the canyon.

You can play the game on GitHub Pages [**HERE**](https://christopher-boustros.github.io/Unity-FPS-Maze-Game/)!

![Alt text](/Game_Screenshot_1.png?raw=true "Game Screenshot 1")

![Alt text](/Game_Screenshot_2.png?raw=true "Game Screenshot 2")

#### Assets used
- *Standard Assets (for Unity 2018.4)* by Unity Technologies
- *Terrain Tools Sample Asset Pack* by Unity Technologies

Due to copyright reasons, these assets are not included in this repository. They must be downloaded from the Unity Asset Store and imported into the Unity project.

## How to run the game

#### Requirements

You must have Unity version 2019.4.9f1 installed on your computer. Other versions of Unity may have compatibility issues.

#### Running the game in Unity

Clone the master branch of this repository with `git clone --single-branch https://github.com/christopher-boustros/Unity-FPS-Maze-Game.git`, or alternatively, download and extract the ZIP archive of the master branch.

Open the Unity Hub, click on the Projects tab, click on the ADD button, and select the root directory of this repository.

Click on the project to open it in Unity.

In the Project window, double click on the `MainScene.unity` file from the `Assets/Project/Scenes` folder to replace the sample scene.

Download and import the *Standard Assets (for Unity 2018.4)* and *Terrain Tools Sample Asset Pack* by Unity Technologies from the Unity Asset Store. Then, delete the `Assets/Standard Assets/Utility/SimpleActivatorMenu.cs` file, which is not supported by Unity version 2019.4.

Click on the play button to play the game. 

## How to play

#### Movement
W = move forward

A = move left

S = move backward

D = move right

Space bar = jump

Left-click = throw projectiles

#### Gameplay
You must pick up projectiles along the path leading to the edge of the canyon. Once at the edge, you must traverse the maze that spans the canyon by jumping between adjacent maze cells to reach the other edge of the canyon. Once you have reached the other edge, you must throw projectiles to destroy one maze cell that is part of the maze solution to make it impossible to traverse the maze again. 

If this is accomplished, the game is won. If instead the player falls into the canyon, runs out of projectiles, or destroys the maze solution before reaching the other side, the game is lost. In either case, a win/loss state is indicated.  

## References

- The implementation of the `MazeCell` class and `InitializeGrid()` method of the `PathGenerator.cs` script is inspired by Richard Hawkes's implementation [here](https://youtu.be/IrO4mswO2o4?t=348).

- The implementation of the `ThrowProjectile.cs` script is inspired by Duan Li's implementation [here](https://github.com/EmolLi/Maze/blob/0e9335605bd70d6155a58c204639a74f1c121002/Assets/Scripts/Shooter.cs).

No code in this repository is copied from the above sources.

## License

This repository is released under the [MIT License](https://opensource.org/licenses/MIT) (see LICENSE).
