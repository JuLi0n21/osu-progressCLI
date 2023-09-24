﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Pooling;
using osu.Game.Rulesets.Objects.Pooling;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace PerformanceCalculatorGUI.Screens.ObjectInspection
{
    internal class OsuObjectInspectorRenderer : PooledDrawableWithLifetimeContainer<OsuObjectInspectorLifetimeEntry, OsuObjectInspectorDrawable>
    {
        private DrawablePool<OsuObjectInspectorDrawable> pool;

        private readonly List<OsuObjectInspectorLifetimeEntry> lifetimeEntries = new();

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                pool = new DrawablePool<OsuObjectInspectorDrawable>(1, 200)
            };
        }

        public void AddDifficultyDataPanel(OsuHitObject hitObject, OsuDifficultyHitObject difficultyHitObject)
        {
            var newEntry = new OsuObjectInspectorLifetimeEntry(hitObject, difficultyHitObject);
            lifetimeEntries.Add(newEntry);
            Add(newEntry);
        }

        public void RemoveDifficultyDataPanel(OsuHitObject hitObject)
        {
            int index = lifetimeEntries.FindIndex(e => e.HitObject == hitObject);

            var entry = lifetimeEntries[index];
            entry.UnbindEvents();

            lifetimeEntries.RemoveAt(index);
            Remove(entry);
        }

        protected override OsuObjectInspectorDrawable GetDrawable(OsuObjectInspectorLifetimeEntry entry)
        {
            var connection = pool.Get();
            connection.Apply(entry);
            return connection;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            foreach (var entry in lifetimeEntries)
                entry.UnbindEvents();
            lifetimeEntries.Clear();
        }
    }
}
