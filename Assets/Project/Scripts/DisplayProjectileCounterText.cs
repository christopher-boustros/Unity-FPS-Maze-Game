/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to Projectile Counter Text
using UnityEngine.UI;
using UnityEngine;

// Displays the projectile counter text
public class DisplayProjectileCounterText : MonoBehaviour
{
    public static int counter = 0;
    public Text counterText;

    // Update is called once per frame
    void Update()
    {
        counterText.text = counter + " Projectiles";
    }
}