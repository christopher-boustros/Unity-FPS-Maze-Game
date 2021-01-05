/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Maze game object
using System.Collections.Generic;
using UnityEngine;

/*
 *  The purpose of this class is to generate a 5 x 5 maze using a 9 x 9 grid.
 *  Below is an example of a 5 x 5 maze represented with an 9 x 9 grid in which there is no path between any two maze cells (no cell has an adjacent cell): 
 *  
 *  O   X   O   X   O   X   O   X   O 
 *  X   X   X   X   X   X   X   X   X
 *  O   X   O   X   O   X   O   X   O
 *  X   X   X   X   X   X   X   X   X
 *  O   X   O   X   O   X   O   X   O
 *  X   X   X   X   X   X   X   X   X
 *  O   X   O   X   O   X   O   X   O
 *  X   X   X   X   X   X   X   X   X
 *  O   X   O   X   O   X   O   X   O
 *  
 *  The Each 'O' is a cell in the 5 x 5 maze, composed within the 9 x 9 grid.
 *  - The 'O's represent maze cells
 *  - The 'X's represent walls/barriers between maze cells, or equivalently the abscence of a path between maze cells
 *  - The '-'s will represent a path betweetween maze cells, or equivalently the abscence of a wall/barrier between maze cells
 *  Two maze cells are adjacent to each other if and only if there is a '-' between them horizontally or vertically
 *  A player can only move between maze cells that are adjacent. This will be enforced in the game by the fact that there will be a floor
    between two adjacent maze cells to allow the player to jump from one maze cell to the floor to the other maze cell, while two maze
    cells will be too far away for a player to jump from one to the other without the floor in between.
 *  
 *  Below is an example of a perfect 5 x 5 maze represented with an 9 x 9 grid:
 *  
 *  O   -   O   X   O   -   O   -   O
 *  -   X   -   X   X   X   X   X   -
 *  O   X   O   -   O   -   O   X   O
 *  -   X   X   X   X   X   -   X   -
 *  O   X   O   -   O   -   O   X   O
 *  X   X   -   X   X   X   X   X   -
 *  O   X   O   X   O   X   O   -   O
 *  -   X   -   X   X   X   -   X   X
 *  O   -   O   X   O   -   O   -   O
 *
 *  To generate a perfect 5 x 5 maze in a 9 x 9 grid, this class first generates it in a 5 x 5 maze and then converts the coordinates of the maze cells from 
    the 5 x 5 grid to the 9 x 9 grid by multiplying the coordinates by 2. For example, a maze cell at coordinates (4, 4) in the 5 x 5 grid will be at
    coordinates (8, 8) in the 9 x 9 grid. 
 */
public class MazeGenerator : MonoBehaviour
{
    // The GridCell class represents a single cell in the 11 x 11 grid that makes up the 5 x 5 maze
    private class GridCell
    {
        public GameObject floor;
        public string type; // The type of cell: 'X', 'O', or '_'
    }

    public GameObject mazeFloor; // The game object used for maze cell floors (the 'O' cells)
    public GameObject adjacencyFloor; // The game object used for the cells between maze cells (the '-' cells)
                                      // The adjacencyFloors will be smaller and lower than the floors that are actually part of the maze
    private readonly float adjacencyFloorHeightOffset = 3; // The amount of distance by which the adjacencyFloors will be lower than the mazeFloors
    private readonly float size = 40f; // The length and width of the floor (should not be lower than the length and width scale of the floor game object)
    private readonly float gridX = 295f, gridY = 133, gridZ = 85f; // The x, y, z coordinates of the center of the grid cell at (row, column)=(0, 0)
    public const int GRID_ROWS = 9, GRID_COLUMNS = 9;
    public const int MAZE_ROWS = 5, MAZE_COLUMNS = 5;
    private static GridCell[,] gridCells; // A 2D array of GridCell objects that represent cells in the rectangular maze with dimensions 9 x 9

    private List<int[,]> maze; // The list of edges in the maze (using 5 x 5 grid coordinates)
    private List<int[]> mazeSolution; // The list of nodes that make up the solution to the maze (a subset of the list of all edges in that maze)
    private static List<int[]> mazeSolutionIncludingAdjacencyFloors; // The maze solution including adjacency floors ('-' cells) using 9 x 9 grid coordinates
    private static bool mazeSolutionDestroyed = false; // True if at least one maze floor or adjacency floor part of the maze solution has been destroyed by a projectile

    // Start is called before the first frame update
    void Start()
    {
        InitializeGrid();
        maze = PerfectMazeAlgorithm();
        CreateMaze(maze); // Maze is created by destroying adjacency floors
        mazeSolution = SolveMaze(maze);
        mazeSolutionIncludingAdjacencyFloors = ConvertMazeSolution(mazeSolution);
    }

    // Create every floor (adjacency floor and maze floor) in the 9 x 9 grid
    private void InitializeGrid()
    {
        gridCells = new GridCell[GRID_ROWS, GRID_COLUMNS]; // Initialize the gridCells array to have 9 rows and 9 columns

        for (int r = 0; r < GRID_ROWS; r++) // For each row in the grid
        {
            for (int c = 0; c < GRID_COLUMNS; c++) // For each cell in the row (at column c)
            {
                gridCells[r, c] = new GridCell(); // Initialize the cell in the array with a GridCell object

                if (r % 2 == 1 || c % 2 == 1) // If the row or column is odd, then it's an 'X' or '-' cell
                {
                    if (r % 2 == 1 && c % 2 == 1) // If both are odd, then this cell cannot be an adjacency floor ('-' cell) because it is not horizontally or vertically between any two maze cells
                    {
                        gridCells[r, c].type = "X"; // Set the cell type
                        continue; // Do not create a floor
                    }
                    else // It can be an adjacency floor
                    {
                        // Create the floor
                        gridCells[r, c].floor = Instantiate(adjacencyFloor, new Vector3(gridX + r * size, gridY - (size / 2f) - adjacencyFloorHeightOffset, gridZ + c * size), Quaternion.identity) as GameObject;
                        gridCells[r, c].type = "-"; // Set the cell type
                    }
                }
                else // It's an 'O' cell
                {
                    // Create the floor
                    gridCells[r, c].floor = Instantiate(mazeFloor, new Vector3(gridX + r * size, gridY - (size / 2f), gridZ + c * size), Quaternion.identity) as GameObject;
                    gridCells[r, c].type = "O"; // Set the cell type
                }

                gridCells[r, c].floor.name = "MazeFloor " + r + "," + c; // Give the floor a name
                gridCells[r, c].floor.transform.Rotate(Vector3.right, 0f); // Set the right rotation to 0 degrees
                gridCells[r, c].floor.transform.parent = transform; // Set the parent of the wall so that it shows up under the Path game objet in Unity
            }
        }
    }

    /* 
	 * PerfectMazeAlgorithm() returns a randomly-generated list of edges in a 5 x 5 grid that represents a perfect 5 x 5 maze
	 * The edges represent paths in the MazeCells array that are walkable (part of the maze)
	 * The list of edges is generated using Prim's algorithm to generate a spanning tree. Any spanning tree is a perfect maze.
	 */
    private List<int[,]> PerfectMazeAlgorithm()
    {
        List<int[]> visitedNodes = new List<int[]>(); // Holds all nodes of the maze that have been visited by Prim's algorithm
        List<int[,]> perfectMaze = new List<int[,]>(); // The list of edges that will be returned
        System.Random random = new System.Random(); // Create an instance of System.Random to generate random numbers

        // A helper method for picking an edge randomly from a list
        int[,] pickRandomNode(List<int[,]> edges)
        {
            int index = random.Next(0, edges.Count);
            return edges[index];
        }

        // A helper method for checking if a node is in a list
        bool nodeInList(int[] node, List<int[]> lst)
        {
            return lst.Exists(x => x[0] == node[0] && x[1] == node[1]);
        }

        /*
		 * Begin Prim's algorithm
		 * Note: instead of picking edges with minimum weight, edges are picked randomly
		 * The algorithm uses a 5 x 5 grid as the graph where each node has coordinates (r, c). There are 25 nodes in total. 
		 */
        int[] startingNode = new int[2] { random.Next(0, MAZE_ROWS), random.Next(0, MAZE_COLUMNS) }; // Pick a starting node randomly
        visitedNodes.Add(startingNode); // Add the starting node 

        while (visitedNodes.Count < MAZE_ROWS * MAZE_COLUMNS) // While not all nodes have been visited
        {
            List<int[,]> allPossibleEdges = new List<int[,]>(); // Create an array to store all possible edges that can be made. 
                                                                // A possible edge is an edge between a visited node and an unvisited node that are adjacent

            foreach (int[] node in visitedNodes)
            {
                // Look for adjacent nodes
                // Since the graph is a grid, a node may have an adjacent node left, right, up, and down.

                if (node[1] - 1 >= 0) // Look left
                {
                    int[] nodeLeft = new int[2] { node[0], node[1] - 1 };
                    if (!nodeInList(nodeLeft, visitedNodes)) // If the left node is not visited
                    {
                        int[,] possibleEdge = new int[,] { { node[0], node[1] }, { nodeLeft[0], nodeLeft[1] } };
                        allPossibleEdges.Add(possibleEdge); // Add the edge from the visited node to nodeLeft to the list of all possible edges
                    }
                }

                if (node[1] + 1 <= MAZE_COLUMNS - 1) // Look right
                {
                    int[] nodeRight = new int[2] { node[0], node[1] + 1 };
                    if (!nodeInList(nodeRight, visitedNodes))
                    {
                        int[,] possibleEdge = new int[,] { { node[0], node[1] }, { nodeRight[0], nodeRight[1] } };
                        allPossibleEdges.Add(possibleEdge);
                    }
                }

                if (node[0] - 1 >= 0) // Look up
                {
                    int[] nodeUp = new int[2] { node[0] - 1, node[1] };
                    if (!nodeInList(nodeUp, visitedNodes))
                    {
                        int[,] possibleEdge = new int[,] { { node[0], node[1] }, { nodeUp[0], nodeUp[1] } };
                        allPossibleEdges.Add(possibleEdge);
                    }
                }

                if (node[0] + 1 <= MAZE_ROWS - 1) // Look down
                {
                    int[] nodeDown = new int[2] { node[0] + 1, node[1] };
                    if (!nodeInList(nodeDown, visitedNodes))
                    {
                        int[,] possibleEdge = new int[,] { { node[0], node[1] }, { nodeDown[0], nodeDown[1] } };
                        allPossibleEdges.Add(possibleEdge);
                    }
                }
            }

            // Pick a random edge from the list of all possible edges
            // Note: instead of picking an edge with minimum weight, we are picking an edge at random since we do not need a minimum spanning tree
            int[,] randomEdge = pickRandomNode(allPossibleEdges);

            // Add the node from the edge to the list of visited nodes
            int[] visitedNode = new int[] { randomEdge[1, 0], randomEdge[1, 1] };
            visitedNodes.Add(visitedNode);

            // Add the edge to the maze
            perfectMaze.Add(randomEdge);
        }

        return perfectMaze;
    }

    /*
	 * CreateMaze() uses the list of edges in a 5 x 5 grid from PerfectMazeAlgorithm() to destroy the '-'s (adjacencyFloors) of the 9 x 9 grid
	 * The adjacencyFloors link maze cells that are adjacent (that have a path between them). So, this method destroys adjacencyFloors that are between two cells
	   which are not part of an edge in the list of edges.
	 * After the adjacencyFloors are destroyed, the maze is complete
	 */
    public void CreateMaze(List<int[,]> edges)
    {
        List<int[]> allAdjacencyFloorCoordinates = new List<int[]>();

        foreach (int[,] edge in edges)
        {
            // Get (r, c) coordinates of the first and second node in the edge and multiply them by 2 to convert them from 5 x 5 to 9 x 9 grid coordinates
            int[] node1 = new int[2] { 2 * edge[0, 0], 2 * edge[0, 1] };
            int[] node2 = new int[2] { 2 * edge[1, 0], 2 * edge[1, 1] };
            int[] adjacencyFloorCoordinates; // The coordinates of the adjacency floor to destroy (the one in between the two nodes in the edge) 

            // Check if the edge is vertical or horizontal
            if (node1[0] - node2[0] == 0 && System.Math.Abs(node1[1] - node2[1]) == 2) // Horizontal edge
            {
                if (node1[1] - node2[1] > 0)
                {
                    adjacencyFloorCoordinates = new int[2] { node2[0], node2[1] + 1 };
                }
                else
                {
                    adjacencyFloorCoordinates = new int[2] { node1[0], node1[1] + 1 };
                }
            }
            else if (System.Math.Abs(node1[0] - node2[0]) == 2 && node1[1] - node2[1] == 0) // Vertical edge
            {
                if (node1[0] - node2[0] > 0)
                {
                    adjacencyFloorCoordinates = new int[2] { node2[0] + 1, node2[1] };
                }
                else
                {
                    adjacencyFloorCoordinates = new int[2] { node1[0] + 1, node1[1] };
                }
            }
            else // Invalid edge
            {
                Debug.Log("Failed to destroy adjacency floors");
                return;
            }

            allAdjacencyFloorCoordinates.Add(adjacencyFloorCoordinates); // Add the coordinates to the list of all adjacency floor coordinates
        }

        // Destroy the grid cells that are not adjacency floors (coordinates do not match one in the allAdjacencyFloorCoordinates list)
        for (int r = 0; r < GRID_ROWS; r++)
        {
            for (int c = 0; c < GRID_COLUMNS; c++)
            {
                if (string.Compare(gridCells[r, c].type, "-") == 0) // If the gridCell is type '-'
                {

                    if (allAdjacencyFloorCoordinates.Exists(x => x[0] == r && x[1] == c)) // If the coordinate of the gridcell matches the coordinates of an adjacency floor
                    {
                        continue; // Do not destroy that adjacency floor
                    }
                    else
                    {
                        Destroy(gridCells[r, c].floor); // Destroy that adjacency floor
                        gridCells[r, c].type = "X"; // Update the type
                    }
                }
            }
        }
    }

    /*
	 * This method finds the unique path (list of nodes) from the start cell to the end cell of the maze.
	 * This path is found using depth-first search
	 */
    public List<int[]> SolveMaze(List<int[,]> maze)
    {
        // Helper method to find the list of nodes adjacent to a node in the maze
        List<int[]> findAdjacencyList(int[] node)
        {
            List<int[]> l = new List<int[]>();

            foreach (int[,] edge in maze)
            {
                int[] n1 = new int[2] { edge[0, 0], edge[0, 1] }; // Get the left node of the edge
                int[] n2 = new int[2] { edge[1, 0], edge[1, 1] }; // Get the right node of the edge

                if (n1[0] == node[0] && n1[1] == node[1]) // If node is equal to n1
                {
                    l.Add(n2); // Add n2 to the list
                }
                else if (n2[0] == node[0] && n2[1] == node[1]) // If node is equal to n2
                {
                    l.Add(n1); // Add n1 to the list
                }

                if (l.Count == 4)
                {
                    break; // Stop once the list has 4 nodes because there cannot be more than 4 adjacent nodes
                }
            }

            return l;
        }

        int[] sourceNode = new int[2] { MAZE_ROWS - 1, 2 }; // The entrance node of the maze
        int[] destinationNode = new int[2] { 0, 2 }; // The exit node of the maze
        List<int[]> currentPath = new List<int[]>(); // Stores the current path for the DFS algorithm
        List<int[]> finalPath = new List<int[]>(); // To store the result of the DFS algorithm
        bool[,] beingVisited = new bool[MAZE_ROWS, MAZE_COLUMNS]; // This list keeps track of which nodes in the maze are being visited for the DFS algorithm

        // Initialize the beingVisited list
        for (int r = 0; r < MAZE_ROWS; r++)
        {
            for (int c = 0; c < MAZE_COLUMNS; c++)
            {
                beingVisited[r, c] = false;
            }
        }

        // Helper method to perform the depth-frist search algorithm on the maze to find the path from the entrance to the exit of the maze
        void depthFirstSearch(int[] node)
        {
            beingVisited[node[0], node[1]] = true; // The node is being visited

            // BASE CASE
            if (node[0] == destinationNode[0] && node[1] == destinationNode[1]) // If the node is the destination node
            {
                finalPath = new List<int[]>(currentPath); // Store the current path
                return; // Stop searching
            }

            // RECURSIVE STEP
            foreach (int[] n in findAdjacencyList(node)) // For each node n adjacent to the current node
            {
                if (!beingVisited[n[0], n[1]]) // If n has not already being visited
                {
                    currentPath.Add(n); // Add n to the current path
                    depthFirstSearch(n); // Perform depthFirstSearch on n
                    currentPath.Remove(n); // Remove n from the current path
                }
            }

            beingVisited[node[0], node[1]] = false; // The node is no longer being visited
        }

        // Perform depth-first search to fill the path list with every node in maze that is in the solution (in the path from the entrance to exit of the maze)
        depthFirstSearch(sourceNode);

        // Add the source node to the beginning of finalPath
        finalPath.Insert(0, sourceNode);

        return finalPath;
    }

    // This method converts the maze solution from 5 x 5 to 9 x 9 grid coordinates and includes adjacency floors
    public List<int[]> ConvertMazeSolution(List<int[]> solution5x5)
    {
        List<int[]> solution9x9 = new List<int[]>(); // The converted solution to be returned

        int r = solution5x5[0][0], c = solution5x5[0][1]; // The previous row and column

        foreach (int[] currentNode in solution5x5)
        {
            // Convert the node from 5 x 5 to 9 x 9 grid coordinates
            int[] convertedNode = new int[2] { 2 * currentNode[0], 2 * currentNode[1] };

            // Skip the first node in solution5x5 (since it has no previous node)
            if (currentNode[0] == r && currentNode[1] == c)
            {
                solution9x9.Add(convertedNode);
                continue;
            }

            // Find the adjacency floor coordinates between the previous node and the current node to solution9x9
            int[] adjacencyFloorCoordinates = new int[2];
            if (currentNode[1] - c == -1) // Moving left
            {
                adjacencyFloorCoordinates = new int[2] { convertedNode[0], convertedNode[1] + 1 };
            }
            else if (currentNode[1] - c == 1) // Moving right
            {
                adjacencyFloorCoordinates = new int[2] { convertedNode[0], convertedNode[1] - 1 };
            }
            else if (currentNode[0] - r == -1) // Moving up
            {
                adjacencyFloorCoordinates = new int[2] { convertedNode[0] + 1, convertedNode[1] };
            }
            else if (currentNode[0] - r == 1) // Moving down
            {
                adjacencyFloorCoordinates = new int[2] { convertedNode[0] - 1, convertedNode[1] };
            }
            else // Invalid path list
            {
                Debug.Log("Failed to convert maze solution");
                return null;
            }

            // Add the adjacency floor coordinates to solution9x9
            solution9x9.Add(adjacencyFloorCoordinates);

            // Add the converted node to solution9x9
            solution9x9.Add(convertedNode);

            // Update the previous row and column
            r = currentNode[0];
            c = currentNode[1];
        }

        return solution9x9;
    }

    // Returns the floor of a grid cell
    public static GameObject GetGridCellFloor(int r, int c)
    {
        if (r >= 0 && r < GRID_ROWS && c >= 0 && c < GRID_COLUMNS)
        {
            return gridCells[r, c].floor;
        }
        else
        {
            return null;
        }
    }

    public static bool IsMazeSolutionDestroyed()
    {
        return mazeSolutionDestroyed;
    }

    public static void SetMazeSolutionDestroyed()
    {
        mazeSolutionDestroyed = true;
    }

    // Returns true if a grid cell at coordinates (r, c) is part of the maze solution
    public static bool IsCellPartOfMazeSolution(int r, int c)
    {
        if (r >= 0 && r < GRID_ROWS && c >= 0 && c < GRID_COLUMNS)
        {
            return mazeSolutionIncludingAdjacencyFloors.Exists(x => x[0] == r && x[1] == c);
        }
        else
        {
            return false;
        }
    }
}