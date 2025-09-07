using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Crux.BufferViewer.Editor.Controls
{
    public class RenderQueueSlider : SliderInt
    {
        private struct Tick
        {
            public int position;
            public VisualElement element;
        }

        private List<Tick> ticks = new();

        public new class UxmlFactory : UxmlFactory<RenderQueueSlider, UxmlTraits>
        {
            public override VisualElement Create(IUxmlAttributes bag, CreationContext cc)
            {
                var field = base.Create(bag, cc) as RenderQueueSlider;

                return field;
            }
        }


        public new class UxmlTraits : SliderInt.UxmlTraits
        {
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as RenderQueueSlider;
            }
        }

        public void AddTick(int position)
        {
            var tick = new Button();

            tick.style.position = Position.Absolute;
            tick.style.width = 4;
            tick.style.height = 12;

            tick.style.marginBottom = 0;
            tick.style.marginLeft = 0;
            tick.style.marginTop = 0;
            tick.style.marginRight = 0;

            tick.style.borderBottomWidth = 0;
            tick.style.borderLeftWidth = 0;
            tick.style.borderTopWidth = 0;
            tick.style.borderRightWidth = 0;

            tick.style.paddingBottom = 0;
            tick.style.paddingLeft = 0;
            tick.style.paddingTop = 0;
            tick.style.paddingRight = 0;

            tick.tooltip = position.ToString();

            tick.style.backgroundColor = Color.gray;

            float t = Mathf.InverseLerp(lowValue, highValue, position);

            tick.style.left = Length.Percent(t * 100);
            tick.style.top = Length.Percent(150);

            this.Q("unity-tracker").Add(tick);

            tick.clicked += () =>
            {
                Debug.Log(position);
                value = position;
            };

            ticks.Add(new()
            {
                element = tick,
                position = position
            });
        }

        public void PreviousRenderer()
        {
            try
            {
                var tick = ticks
                    .OrderBy(tick => tick.position)
                    .Last(tick => tick.position < value);

                value = tick.position;
            }
            catch (InvalidOperationException e)
            {
            }
        }

        public void NextRenderer()
        {
            try
            {
                var tick = ticks
                    .OrderBy(tick => tick.position)
                    .First(tick => tick.position > value);

                value = tick.position;
            }
            catch (InvalidOperationException e)
            {
            }
        }

        public void Back()
        {
            value -= 1;
        }

        public void Forward()
        {
            value += 1;
        }

        public void ClearTicks()
        {
            foreach (var tick in ticks)
            {
                tick.element.parent.Remove(tick.element);
            }

            ticks.Clear();
        }
    }
}