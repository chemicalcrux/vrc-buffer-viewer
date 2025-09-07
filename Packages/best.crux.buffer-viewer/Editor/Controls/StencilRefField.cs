using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Crux.BufferViewer.Editor.Controls
{
    public class StencilRefField : BindableElement, INotifyValueChanged<int>
    {
        private int stencilRef;

        private Label label;
        private SliderInt slider;
        private List<Toggle> toggles = new();

        public StencilRefField() : this("Stencil Ref")
        {
            
        } 
        
        public StencilRefField(string labelText)
        {
            slider = new SliderInt(0, 255);
            slider.showInputField = true;

            label = new Label(labelText);

            Add(label);

            slider.RegisterValueChangedCallback(evt =>
            {
                value = slider.value;
            });

            Add(slider);
            
            var row = new VisualElement();

            row.style.flexDirection = FlexDirection.Row;
            
            for (int i = 0; i < 8; ++i)
            {
                var toggle = new Toggle();
                
                toggles.Add(toggle);
                toggle.RegisterValueChangedCallback(evt =>
                {
                    value = GetToggleValue();
                });

                toggle.style.flexDirection = FlexDirection.Column;
                toggle.style.alignItems = Align.Center;
                toggle.style.overflow = Overflow.Visible;

                var toggleLabel = new Label((1 << i).ToString());
                toggleLabel.style.fontSize = 10;
                toggleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                toggleLabel.style.maxWidth = 0;
                
                toggle.Add(toggleLabel);
            }

            toggles.Reverse();

            foreach (var toggle in toggles)
            {
                row.Add(toggle);
            }

            toggles.Reverse();

            Add(row);
        }
        
        public new class UxmlFactory : UxmlFactory<StencilRefField, UxmlTraits>
        {
            public override VisualElement Create(IUxmlAttributes bag, CreationContext cc)
            {
                var field = base.Create(bag, cc) as StencilRefField;

                return field;
            }
        }

        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as StencilRefField;
            }
        }

        public void SetValueWithoutNotify(int newValue)
        {
            stencilRef = newValue;

            slider.SetValueWithoutNotify(stencilRef);

            for (int i = 0; i < toggles.Count; ++i)
            {
                int and = stencilRef & (1 << i);
                toggles[i].SetValueWithoutNotify(and != 0);
            }
        }

        private int GetToggleValue()
        {
            int result = 0;
            
            for (int i = 0; i < toggles.Count; ++i)
            {
                if (toggles[i].value)
                    result += 1 << i;
            }

            return result;
        }
        
        public int value
        {
            get => stencilRef;
            set
            {
                int previousValue = stencilRef;
                SetValueWithoutNotify(value);
                using ChangeEvent<int> pooled = ChangeEvent<int>.GetPooled(previousValue, stencilRef);
                pooled.target = this;
                SendEvent(pooled);
            }
        }
    }
}