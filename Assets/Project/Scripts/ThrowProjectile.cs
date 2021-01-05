/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Main Camera game object
// The implementation of this script is inspired by Duan Li's implementation: https://github.com/EmolLi/Maze/blob/0e9335605bd70d6155a58c204639a74f1c121002/Assets/Scripts/Shooter.cs
using UnityEngine;

// Allows the player to throw a projectile
public class ThrowProjectile : MonoBehaviour
{
    public GameObject projectile; // The projectile to be thrown
    public AudioClip sound; // The sound to be played when the projectile is thrown
    private readonly float speed = 150f; // The speed at which the projectile is thrown
    private readonly float time = 10f; // The time before the projectile is destroyed after being thrown
    private readonly float volume = 2f; // The volume of the sound

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        { // If "Fire1" button is pressed (the left mouse button or left control key)
            /*
			 * VERIFY IF PROJECTILE CAN BE THROWN
			 */
            if (GameObject.FindWithTag("ProjectileInAir"))
            { // If there exists a projectile object in the air
                return; // Do not throw a projectile since a projectile has already been thrown and has not yet been destroyed
            }

            // If the game is over
            if (DetectWinLossState.gameIsOver)
            {
                return; // Do not throw a projectile
            }

            if (DisplayProjectileCounterText.counter <= 0) // If the projectile counter variable is not at least 1
            {
                return; // Do not throw a projectile since there are no projectiles available to throw
            }

            /*
			 * THROW PROJECTILE
			 */
            // Instantiante a new projectile in front of the camera
            GameObject newProjectile = Instantiate(projectile, transform.position + 5 * transform.forward, transform.rotation) as GameObject;
            newProjectile.name = "ProjectileInAir";
            newProjectile.tag = "ProjectileInAir"; // Set the tag so that the projectile can be detected when the player tries to throw another projectile

            DisplayProjectileCounterText.counter--; // Decrement the projectile counter variable

            // Add force to make the new projectile move
            newProjectile.GetComponent<Rigidbody>().AddForce(transform.forward * speed, ForceMode.VelocityChange);
            Destroy(newProjectile, time); // Destroy the new projectile after a certain amount of time

            /*
			 * PLAY SOUND
			 */
            AudioSource.PlayClipAtPoint(sound, newProjectile.transform.position, volume);
        }
    }
}
