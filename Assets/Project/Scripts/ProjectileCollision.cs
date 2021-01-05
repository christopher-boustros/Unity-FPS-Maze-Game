/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Projectile game object prebab
using UnityEngine;

// Detects and handles collisions with a projectile
public class ProjectileCollision : MonoBehaviour
{
    public AudioClip collectionSound; // The sound to be played when the projectile is collected
    public AudioClip collisionSound; // The sound played when the projectile collides with an object other than the player

    // This method is called if a collision occurs with the projectile ("Is Trigger" must be checked in te projectile's collider)
    void OnTriggerEnter(Collider collider)
    {
        if (collider.name == "Player" || collider.name == "MainCamera" || collider.name == "FPSController") // If the projectile collided with the player
        {
            if (gameObject.name == "ProjectileOnPath") // If the projectile was on the path (not in the air)
            {
                Destroy(gameObject); // Destroy the projectile	
                DisplayProjectileCounterText.counter++; // Increment the projectile counter variable

                if (collectionSound) // If a collection sound is chosen
                {
                    AudioSource.PlayClipAtPoint(collectionSound, gameObject.transform.position, 4f); // Play the sound
                }
            }
        }
        else if (gameObject.name == "ProjectileOnPath") // If the projectile was on the path and did not collide with the player
        {
            return; // Do nothing
        }
        else // Projecile collided with an object other than the player
        {
            Destroy(gameObject); // Destroy the projectile

            if (collisionSound) // If a collision sound is chosen
            {
                AudioSource.PlayClipAtPoint(collisionSound, gameObject.transform.position, 4f); // Play the sound
            }

            if (collider.name.Length >= 13 && collider.name.Substring(0, 10) == "MazeFloor ") // If it collided with a maze floor
            {
                // Find the maze floor object
                GameObject mazeFloor = GameObject.Find(collider.name);

                // Try to find the corresponding grid cell
                int[] coordinates = new int[2];
                bool found = false;
                for (int r = 0; r < MazeGenerator.GRID_ROWS; r++)
                {
                    if (found)
                    {
                        break;
                    }

                    for (int c = 0; c < MazeGenerator.GRID_COLUMNS; c++)
                    {
                        ;
                        if (MazeGenerator.GetGridCellFloor(r, c) == mazeFloor)
                        {
                            coordinates[0] = r;
                            coordinates[1] = c;
                            found = true;
                            break;
                        }

                    }
                }

                if (found) // If the game object is actually a maze floor (cooresponding grid cell was found)
                {
                    Destroy(mazeFloor); // Destroy the maze floor object

                    if (MazeGenerator.IsCellPartOfMazeSolution(coordinates[0], coordinates[1])) // If the maze floor was part of the maze solution
                    {
                        MazeGenerator.SetMazeSolutionDestroyed(); // Indicates that the maze solution has been destroyed
                    }
                }
            }
        }
    }
}
