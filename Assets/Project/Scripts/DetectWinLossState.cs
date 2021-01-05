/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Win Loss State Text
using UnityEngine;

// Detects when the player wins or loses the game
public class DetectWinLossState : MonoBehaviour
{
    public static bool reachedOtherSideOnce = false;
    public static string winLossState = "";
    private static GameObject player;

    public static bool gameIsOver = false; // True if the player wins or loses

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player"); // Get the player game object
    }

    // Update is called once per frame
    void Update()
    {
        if (gameIsOver)
        {
            return; // Do nothing
        }

        // Set reachedOtherSideOnce
        if (player.transform.position.x < 265f && player.transform.position.y > 100f)
        {
            reachedOtherSideOnce = true;
        }

        // Player has fallen into the canyon
        bool failCase1 = player.transform.position.y < 50f;

        // Player has no more projectiles and has not destroyed the maze solution
        bool failCase2 = DisplayProjectileCounterText.counter <= 0 && !MazeGenerator.IsMazeSolutionDestroyed();

        // Player has destroyed the maze solution before reaching the other side
        bool failCase3 = MazeGenerator.IsMazeSolutionDestroyed() && !reachedOtherSideOnce;

        // Player has reached the other side and destroyed the maze solution
        bool winCase = MazeGenerator.IsMazeSolutionDestroyed() && reachedOtherSideOnce;


        if (failCase1)
        {
            winLossState = "You Lost!\nReason: You fell into the canyon";
            GameOver();
        }
        else if (failCase2)
        {
            bool noMoreProjectilesOnPath = GameObject.FindWithTag("ProjectileOnPath") == null; // True if there are no  more projectiles on the path
            bool noProjectileInAir = GameObject.FindWithTag("ProjectileInAir") == null; // True if there is no projectile currently in the air

            if (noMoreProjectilesOnPath && noProjectileInAir)
            {
                winLossState = "You Lost!\nReason: You have no more projectiles, there are no more projectiles on the floor, and you have not destroyed the maze solution";
                GameOver();
            }
            else if (!noMoreProjectilesOnPath)
            {
                winLossState = "Pick up projectiles from the green path";
            }
        }
        else if (failCase3)
        {
            winLossState = "You Lost!\nReason: You destroyed the maze solution before reaching the other side";
            GameOver();
        }
        else if (winCase)
        {
            if (player.transform.position.x < 265f && player.transform.position.y > 100f) // If the player is still on the other side
            {
                winLossState = "You Win!";
                GameOver();
            }
            else
            {
                winLossState = "You Lost!\nReason: You destroyed the maze solution but you did not stay on the other side of the canyon";
                GameOver();
            }
        }
        else
        {
            winLossState = "";
        }
    }

    // Stops the player from performing any further action and uses a new camera
    public void GameOver()
    {
        gameIsOver = true;

        GameObject fpscontroller = GameObject.FindWithTag("GameController");
        fpscontroller.GetComponent<UnityStandardAssets.Characters.FirstPerson.FirstPersonController>().enabled = false; // Disable the first person controller script

        // Change the position of the fpscontroller
        fpscontroller.transform.position = new Vector3(499f, 994.3f, 278.4f);

        // Change the rotation of the fpscontroller
        Quaternion rotation = fpscontroller.transform.rotation;
        rotation.eulerAngles = new Vector3(0f, 0f, 0f);
        fpscontroller.transform.rotation = rotation;

        // Change the rotation of the camera
        GameObject camera = GameObject.FindWithTag("MainCamera");
        camera.transform.localPosition = new Vector3(0f, 0f, 0f);

        // Change the rotation of the camera
        Quaternion cameraRotation = camera.transform.rotation;
        cameraRotation.eulerAngles = new Vector3(89f, 0f, 0f);
        camera.transform.rotation = cameraRotation;

        // Destroy the projectile in the air (if it exists)
        GameObject projectileInAir = GameObject.FindWithTag("ProjectileInAir");
        if (projectileInAir)
        {
            Destroy(projectileInAir);
        }
    }
}
