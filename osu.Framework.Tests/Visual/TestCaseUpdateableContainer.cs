﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Testing;
using System.Threading;

namespace osu.Framework.Tests.Visual
{
    public class TestCaseUpdateableContainer : TestCase
    {
        public TestCaseUpdateableContainer()
        {
            TestUpdateableContainer updateContainer;
            PlaceholderTestUpdateableContainer placeholderContainer;
            DelayedTestUpdateableContainer delayedContainer;

            AddRange(new Drawable[]
            {
                updateContainer = new TestUpdateableContainer
                {
                    Position = new Vector2(50, 50),
                    Size = new Vector2(100, 100)
                },
                placeholderContainer = new PlaceholderTestUpdateableContainer
                {
                    Position = new Vector2(50, 250),
                    Size = new Vector2(100, 100)
                },
                delayedContainer = new DelayedTestUpdateableContainer
                {
                    Position = new Vector2(50, 450),
                    Size = new Vector2(100, 100)
                }
            });


            addNullTest("no PH", updateContainer, false);
            addItemTest("no PH", updateContainer, 0);
            addItemTest("no PH", updateContainer, 1);
            addNullTest("no PH", updateContainer, false);

            addNullTest("PH", placeholderContainer, true);
            addItemTest("PH", placeholderContainer, 0);
            addItemTest("PH", placeholderContainer, 1);
            addNullTest("PH", placeholderContainer, true);

            AddStep("Set item null", () => delayedContainer.Item = null);
            AddStep("Set item with delay", () => delayedContainer.Item = new TestItem(0));
            AddAssert("Test next drawable not null", () => delayedContainer.NextDrawable != null);
            AddWaitStep(5);
            AddAssert("Test next drawable null", () => delayedContainer.NextDrawable == null);
        }

        private void addNullTest(string prefix, TestUpdateableContainer container, bool expectPlaceholder)
        {
            AddStep($"{prefix} Set null", () => container.Item = null);
            if (expectPlaceholder)
                AddAssert($"{prefix} Check null with PH", () => container.DisplayedDrawable == null && (container.PlaceholderDrawable?.Alpha ?? 0) > 0);
            else
            {
                AddUntilStep(() => container.NextDrawable == null, $"{prefix} wait until loaded");
                AddAssert($"{prefix} Check non-null no PH", () => container.VisibleItemId == -1 && container.PlaceholderDrawable == null);
            }
        }

        private void addItemTest(string prefix, TestUpdateableContainer container, int itemNumber)
        {
            AddStep($"{prefix} Set item {itemNumber}", () => container.Item = new TestItem(itemNumber));
            AddUntilStep(() => container.NextDrawable == null, $"{prefix} wait until loaded");
            AddAssert($"{prefix} Check item {itemNumber}", () => container.VisibleItemId == itemNumber);
        }

        private class TestItem
        {
            public readonly int ItemId;

            public TestItem(int itemId)
            {
                ItemId = itemId;
            }
        }

        private class TestItemDrawable : SpriteText
        {
            public readonly int ItemId;

            public TestItemDrawable(TestItem item)
            {
                ItemId = item?.ItemId ?? -1;
                Position = new Vector2(10, 10);
                Text = item == null ? "No Item" : $"Item {item.ItemId}";
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                // delay
                Thread.Sleep((int)(500 / Clock.Rate));
            }
        }

        private class TestUpdateableContainer : UpdateableContainer<TestItem>
        {
            public TestItem Item { get => Source; set => Source = value; }

            public int VisibleItemId => (DisplayedDrawable as TestItemDrawable)?.ItemId ?? -1;

            public TestUpdateableContainer()
                : base((lhs, rhs) => lhs?.ItemId == rhs?.ItemId ? 0 : -1)
            {
                Add(new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    AlwaysPresent = true
                });
                BorderColour = Color4.White;
                BorderThickness = 2;
                Masking = true;
            }

            protected override Drawable CreateDrawable(TestItem item) => new TestItemDrawable(item);
        }

        private class PlaceholderTestUpdateableContainer : TestUpdateableContainer
        {
            protected override Drawable CreateDrawable(TestItem item) => item == null ? null : new TestItemDrawable(item);

            protected override Drawable CreatePlaceholder() => new Box { Colour = Color4.Blue };
        }

        private class DelayedTestUpdateableContainer : PlaceholderTestUpdateableContainer
        {
            protected override double LoadDelay => 500 / Clock.Rate;
        }
    }
}
