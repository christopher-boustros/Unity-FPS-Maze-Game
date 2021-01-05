/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Path game object
// The implementation of the the MazeCell class and InitializeGrid() method is inspired by Richard Hawke's implementation: https://www.youtube.com/watch?v=IrO4mswO2o4&ab_channel=RichardHawkes
using System.Collections.Generic;
using UnityEngine;

// Generates the unicursal path that leads to the edge of the canyon
public class PathGenerator : MonoBehaviour
{
    public GameObject wallPrefab; // The game object with which the maze walls are made up of
    public GameObject floorPrefab; // The game object with which the maze floors are made up of
    public GameObject projectilePrefab; // The projectile to add to the path
    private static readonly int mazeRows = 10, mazeColumns = 4; // Number of rows and columns in the rectangular maze
    private static readonly float cellSize = 15f; // The length, width, and height of the maze cells (must match the x and y scale of the floor and wall game object)
    private static readonly float mazeX = 715f, mazeY = 134f, mazeZ = 223f; // The x, y, z coordinates of the center of the maze cell at (row, column)=(0, 0)
    private static readonly System.Random random = new System.Random(); // Create an instance of System.Random to generate random numbers

    private MazeCell[,] mazeCells; // A 2D array of MazeCell objects that represent cells in the rectangular maze with dimensions mazeRows x mazeColumns

    // The MazeCell class represents a single cell in a maze
    private class MazeCell
    {
        // The floors and walls of the cell
        public GameObject floor;
        public GameObject[] walls = new GameObject[4]; // The 4 walls of the maze cell in the following order: north, east, south, west
        
        // The (row, col) grid coordinates of the cell
        public int row;
        public int col;

        // Half the size of a cell
        public float radius = cellSize / 2f;

        // The center (x, y, z) Unity coordinates of the cell
        public Vector3 position;

        // Constructor
        public MazeCell(int row, int col, GameObject wallPrefab, GameObject floorPrefab, Transform path)
        {
            this.row = row;
            this.col = col;
            PositionInit(row, col); // Initialize position

            floor = Instantiate(floorPrefab, position - Vector3.up * radius, Quaternion.Euler(90f, 0f, 0f)); // Instantiate a floor
            walls[0] = Instantiate(wallPrefab, position + Vector3.forward * radius, Quaternion.Euler(0f, 0f, 0f)); // Instantiate the north wall
            walls[1] = Instantiate(wallPrefab, position + Vector3.right * radius, Quaternion.Euler(0f, 90f, 0f)); // Instantiate the east wall

            // Every cell has a floor and two adjacent walls
            // The only cells with three walls are the cells with row = 0 or col = 0
            if (row == 0)
            {
                walls[2] = Instantiate(wallPrefab, position - Vector3.right * radius, Quaternion.Euler(0f, 90f, 0f)); // Instantiate the west wall
            }

            if (col == 0)
            {
                walls[3] = Instantiate(wallPrefab, position - Vector3.forward * radius, Quaternion.Euler(0f, 0f, 0f)); // Instantiate the south wall
            }

            // Create a parent game object for the floor and walls
            GameObject parent = new GameObject();
            parent.name = "Cell (" + row + ", " + col + ")";
            parent.transform.parent = path;

            // For each wall and the floor
            for (int i = -1; i < 4; i++)
            {
                GameObject obj;

                if (i == -1) // The floor
                {
                    obj = floor;
                    obj.name = "Floor";
                }
                else // The walls
                {
                    obj = walls[i];

                    if (obj == null)
                    {
                        continue;
                    }

                    obj.name = "Wall " + i;
                }

                obj.transform.parent = parent.transform; // Set the parent of the object
            }
        }

        // Initialize position
        private void PositionInit(int row, int col)
        {
            position = new Vector3(mazeX + row * cellSize, mazeY, mazeZ + col * cellSize);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializeGrid();
        List<int[]> path = UnicursalMazeAlgorithm();
        CreateMaze(path); // Maze is created by destroying walls
    }

    // Create every possible wall inside the maze (the result is a grid with dimensions mazeRows x mazeColumns)
    private void InitializeGrid()
    {
        mazeCells = new MazeCell[mazeRows, mazeColumns]; // Initialize the mazeCells array to have mazeRows rows and mazeColumns columns

        for (int r = 0; r < mazeRows; r++) // For each row in the maze
        {
            for (int c = 0; c < mazeColumns; c++) // For each cell in the row (at column c)
            {
                mazeCells[r, c] = new MazeCell(r, c, wallPrefab, floorPrefab, transform); // Create a MazeCell object
            }
        }
    }

    /* 
	 * UnicursalMazeAlgorithm() returns a randomly-generated list of coordinates that represents a unicursal maze
	 * The coordinates represent cells in the MazeCells array that are walkable (part of the unicursal path)
	 * For example: the list {[0,0], [0,1], [0,2], ...} represents a maze with a path from cell [0,0] to [0,1] to [0,2] to ...
	 * Algorithm: Start at cell (0,0). Then, either move left, right, or down if possible (do not move to a cell that has already been visited). Repeat until you cannot move anywhere.
	 */
    private List<int[]> UnicursalMazeAlgorithm()
    {
        List<int[]> unicursalMaze = new List<int[]>(); // The list to be returned
        int r = 0, c = 0; // The current row and column
        bool mazeNotDone = true;

        while (mazeNotDone)
        {
            List<int> choices = new List<int>(); // A list containing 3 choices: choice 1 --> move left, choice 2 --> move right, and choice 3 --> move down
            choices.Add(1);
            choices.Add(2);
            choices.Add(3);

            int[] point = new int[2] { r, c };
            unicursalMaze.Add(point); // Add current point to unicursalMaze list

            while (true)
            {
                int randomChoice = choices[random.Next(0, choices.Count)]; // Get a random number from the Choices list
                                                                           // Depending on the choice, we will move left, right, or down from the current cell at (r, c)

                if (randomChoice == 1) // CHOICE 1: move left (r, c) --> (r, c - 1)
                {
                    int[] pointLeft = new int[2] { r, c - 1 };
                    if (c - 1 >= 0 && !unicursalMaze.Exists(x => x[0] == pointLeft[0] && x[1] == pointLeft[1])) // Check if you can move left
                    {
                        c = c - 1; // Move left
                        break;
                    }
                    else
                    {
                        // Do not move left
                        choices.Remove(1); // Remove this option from the list of choices
                    }
                }
                else if (randomChoice == 2) // CHOICE 2: move right (r, c) --> (r, c + 1)
                {
                    int[] pointRight = new int[2] { r, c + 1 };
                    if (c + 1 <= mazeColumns - 1 && !unicursalMaze.Exists(x => x[0] == pointRight[0] && x[1] == pointRight[1])) // Check if you can move right
                    {
                        c = c + 1; // Move right
                        break;
                    }
                    else
                    {
                        // Do not move right
                        choices.Remove(2); // Remove this option from the list of choices
                    }
                }
                else // CHOICE 3: To move down (r, c) --> (r + 1, c)
                {
                    int[] pointDown = new int[2] { r + 1, c };
                    if (r + 1 <= mazeRows - 1 && !unicursalMaze.Exists(x => x[0] == pointDown[0] && x[1] == pointDown[1])) // Check if you can move down
                    {
                        r = r + 1; // Move down
                        break;
                    }
                    else
                    {
                        // Do not move down
                        choices.Remove(3); // Remove this option from the list of choices
                    }
                }

                if (choices.Count == 0) // If all 3 options are not possible
                {
                    // Maze is done
                    mazeNotDone = false;
                    break;
                }
            }
        }

        return unicursalMaze;
    }

    /*
	 * CreateMaze() uses the list generated from UnicursalMazeAlgorithm() to destroy the walls of the grid that intersect with the points in the list
	 * After the walls are destroyed, the maze is complete
	 * This method also changes the color of the floors that are part of the maze solution
	 * This method also adds projectiles along the maze solution
	 */
    public void CreateMaze(List<int[]> path)
    {
        int r = path[0][0], c = path[0][1]; // The previous row and column

        List<int> arr = new List<int>(); // An array that will be used to decide whether to place a projectile randomly

        // Initialize arr
        for (int i = 0; i < path.Count; i++)
        {
            arr.Add(i);
        }

        foreach (int[] currentPoint in path)
        {
            if (currentPoint[0] == 0 && currentPoint[1] == 0)
            {
                continue; // Skip the first point in the path
            }

            if (currentPoint[1] - c == -1) // Moving left
            {
                // Destroy the eastWall of currentPoint
                Destroy(mazeCells[currentPoint[0], currentPoint[1]].walls[0]);
            }
            else if (currentPoint[1] - c == 1) // Moving right
            {
                // Destroy the eastWall of the previous point (r, c)
                Destroy(mazeCells[r, c].walls[0]);
            }
            else if (currentPoint[0] - r == 1) // Moving down
            {
                // Destroy the southWall of the previous point (r, c)
                Destroy(mazeCells[r, c].walls[1]);
            }
            else // Invalid path list
            {
                Debug.Log("Failed to destroy walls");
                return;
            }

            // Update the previous row and column
            r = currentPoint[0];
            c = currentPoint[1];

            // Change color of floor of currentPoint
            mazeCells[r, c].floor.GetComponent<Renderer>().material.color = Color.green;

            int arrMax = 9; // The maximum number of projectiles to create (may be one less since this code does not run for point (0,0))
                            // Add projectile if a random number chosen from arr is less than arrMax
            int randomIndex = random.Next(0, arr.Count);
            if (arr[randomIndex] < arrMax)
            {
                GameObject newProjectile = Instantiate(projectilePrefab, mazeCells[r, c].floor.transform.position + 5 * transform.up, mazeCells[r, c].walls[1].transform.rotation) as GameObject;
                newProjectile.name = "ProjectileOnPath";
                newProjectile.tag = "ProjectileOnPath";
            }
            arr.RemoveAt(randomIndex);
        }

        mazeCells[0, 0].floor.GetComponent<Renderer>().material.color = Color.green; // Change the color of the last floor
        Destroy(mazeCells[r, c].walls[1]); // Create the entrance of the maze
        Destroy(mazeCells[0, 0].walls[2]); // Create the exit of the maze
    }
}
