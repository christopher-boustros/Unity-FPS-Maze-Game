/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to the Projectile game object prefab
using UnityEngine;

// Makes a projectile rotate constantly
public class RotateProjectile : MonoBehaviour
{
    private readonly Vector3 rotation = new Vector3(0f, 100f, 0f); // The amount by which to rotate every second

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotation * Time.deltaTime); // Makes the object rotate
    }
}
