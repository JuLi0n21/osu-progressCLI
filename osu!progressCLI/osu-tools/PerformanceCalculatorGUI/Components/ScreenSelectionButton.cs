﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osu.Game.Overlays.Toolbar;
using osuTK;

namespace PerformanceCalculatorGUI.Components
{
    internal class ScreenSelectionButton : ToolbarButton
    {
        public ScreenSelectionButton(string title, IconUsage? icon = null, GlobalAction? hotkey = null)
        {
            Width = PerformanceCalculatorSceneManager.CONTROL_AREA_HEIGHT;
            Hotkey = hotkey;
            TooltipMain = title;

            SetIcon(new ScreenSelectionButtonIcon(icon) { IconSize = new Vector2(25) });
        }
    }
}
