﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;

namespace PerformanceCalculatorGUI
{
    /// <summary>
    /// Receives callbacks upon .NET Hot Reloads.
    /// </summary>
    internal static class HotReloadCallbackReceiver
    {
        public static event Action<Type[]> CompilationFinished;
        public static void UpdateApplication([CanBeNull] Type[] updatedTypes) => CompilationFinished?.Invoke(updatedTypes);
    }
}
