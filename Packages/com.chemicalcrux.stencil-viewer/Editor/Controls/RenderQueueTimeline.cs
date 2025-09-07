using UnityEngine.UIElements;

namespace ChemicalCrux.StencilViewer.Editor.Controls
{
    public class RenderQueueTimeline : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<RenderQueueTimeline, UxmlTraits>
        {
            public override VisualElement Create(IUxmlAttributes bag, CreationContext cc)
            {
                var field = base.Create(bag, cc) as RenderQueueTimeline;

                var slider = new RenderQueueSlider();

                slider.highValue = 5000;
                slider.showInputField = true;
                slider.bindingPath = field.BindingPath;

                slider.style.marginBottom = 24;

                slider.label = "Render Queue";
                field.Add(slider);

                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;

                field.Add(row);

                var previousRendererButton = new Button(slider.PreviousRenderer);
                previousRendererButton.text = "<<";

                var backButton = new Button(slider.Back);
                backButton.text = "<";

                var forwardButton = new Button(slider.Forward);
                forwardButton.text = ">";

                var nextRendererButton = new Button(slider.NextRenderer);
                nextRendererButton.text = ">>";
                
                row.Add(previousRendererButton);
                row.Add(backButton);
                row.Add(forwardButton);
                row.Add(nextRendererButton);

                return field;
            }
        }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private readonly UxmlStringAttributeDescription bindingPath = new()
                { name = "slider-binding-path", defaultValue = "" };
            
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as RenderQueueTimeline;

                ate!.BindingPath = bindingPath.GetValueFromBag(bag, cc);
            }
        }
            
        public string BindingPath { get; set; }
    }
}