using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Crux.BufferViewer.Editor.Controls
{
    public class BitmaskField : BindableElement, INotifyValueChanged<int>
    {
        private int stencilRef;

        private readonly SliderInt slider;
        private readonly List<Toggle> toggles = new();

        private Label label;
        
        public BitmaskField()
        {
            slider = new SliderInt(0, 255);
            slider.showInputField = true;

            label = new Label();

            Add(label);

            slider.RegisterValueChangedCallback(_ =>
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
                toggle.RegisterValueChangedCallback(_ =>
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
        
        public new class UxmlFactory : UxmlFactory<BitmaskField, UxmlTraits>
        {
            public override VisualElement Create(IUxmlAttributes bag, CreationContext cc)
            {
                var field = base.Create(bag, cc) as BitmaskField;

                field.label.text = field.LabelText;
                
                return field;
            }
        }

        public new class UxmlTraits : BindableElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription label = new()
                { name = "label", defaultValue = "" };
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as BitmaskField;

                ate!.LabelText = label.GetValueFromBag(bag, cc);
            }
             
        }

        private string LabelText { get; set; }

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