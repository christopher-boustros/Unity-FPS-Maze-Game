/*
 * Copyright (c) 2020 Christopher Boustros <github.com/christopher-boustros>
 * SPDX-License-Identifier: MIT
 */
// This script is linked to Win Loss State Text
using UnityEngine.UI;
using UnityEngine;

// Displays the win/loss state text
public class DisplayWinLossStateText : MonoBehaviour
{
    public Text winLossStateText;

    // Update is called once per frame
    void Update()
    {
        winLossStateText.text = DetectWinLossState.winLossState;
    }
}