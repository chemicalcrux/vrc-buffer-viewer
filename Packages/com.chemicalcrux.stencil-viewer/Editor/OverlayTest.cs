using ChemicalCrux.StencilViewer.Editor.Controls;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ChemicalCrux.StencilViewer.Editor
{
    public enum ViewerMode
    {
        ShowBuffer,
        ShowMatching
    }

    [Overlay(typeof(SceneView), "Stencil Viewer")]
    public class OverlayTest : Overlay
    {
        private Mesh mesh;
        private Material activeMaterial;

        private Material stencilViewerMaterial;
        private Material stencilMatcherMaterial;

        private Material displayResultMaterial;

        private bool activated;

        private ViewerMode mode;

        private int renderQueue;
        private int stencilRef;
        private float opacity;

        private static readonly int StencilRef = Shader.PropertyToID("_StencilRef");
        private static readonly int Opacity = Shader.PropertyToID("_Opacity");

        private SliderInt renderQueueField;
        private StencilRefField stencilRefField;

        void Draw(Camera camera)
        {
            if (!displayed)
                return;

            if (!activated)
                return;

            if (!(mesh && activeMaterial))
                return;

            activeMaterial.renderQueue = renderQueue;
            activeMaterial.SetInteger(StencilRef, stencilRef);

            displayResultMaterial.SetFloat(Opacity, opacity);

            var cameraTransform = camera.transform;
            Vector3 pos = cameraTransform.position + cameraTransform.forward * camera.nearClipPlane * 1.1f;

            Matrix4x4 mat =
                Matrix4x4.TRS(pos, cameraTransform.rotation, cameraTransform.lossyScale);

            RenderParams rp = new RenderParams(activeMaterial)
            {
                camera = camera
            };

            Graphics.RenderMesh(rp, mesh, 0, mat);

            rp = new RenderParams(displayResultMaterial)
            {
                camera = camera
            };
            Graphics.RenderMesh(rp, mesh, 0, mat);
        }

        public override void OnCreated()
        {
            base.OnCreated();

            Camera.onPreCull -= Draw;
            Camera.onPreCull += Draw;

            mesh = new Mesh
            {
                vertices = new Vector3[]
                {
                    new(-1, -1, 0),
                    new(1, -1, 0),
                    new(1, 1, 0),
                    new(-1, 1, 0)
                },
                triangles = new[]
                {
                    0,
                    3,
                    1,
                    1,
                    3,
                    2
                }
            };

            stencilViewerMaterial = new Material(Shader.Find("Hidden/chemicalcrux/Stencil Viewer/Stencil View"));
            stencilMatcherMaterial = new Material(Shader.Find("Hidden/chemicalcrux/Stencil Viewer/Stencil Match"));

            displayResultMaterial = new Material(Shader.Find("Hidden/chemicalcrux/Stencil Viewer/Display Result"));
        }

        public override void OnWillBeDestroyed()
        {
            base.OnWillBeDestroyed();

            Object.DestroyImmediate(displayResultMaterial);

            Object.DestroyImmediate(stencilViewerMaterial);
            Object.DestroyImmediate(stencilMatcherMaterial);

            Object.DestroyImmediate(mesh);

            Camera.onPreCull -= Draw;
        }

        private void SetMode(ViewerMode newMode)
        {
            stencilRefField.style.display = DisplayStyle.None;

            mode = newMode;

            switch (mode)
            {
                case ViewerMode.ShowBuffer:
                    activeMaterial = stencilViewerMaterial;
                    break;
                case ViewerMode.ShowMatching:
                    stencilRefField.style.display = DisplayStyle.Flex;
                    activeMaterial = stencilMatcherMaterial;
                    break;
                default:
                    Debug.LogWarning("I don't know how you picked an invalid mode");
                    break;
            }
        }

        public override VisualElement CreatePanelContent()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.chemicalcrux.stencil-viewer/UI/Overlay.uxml");

            var root = uxml.Instantiate();

            var toggle = root.Q<Toggle>("Active");
            toggle.RegisterValueChangedCallback(evt => { activated = evt.newValue; });

            var modeDropdown = root.Q<EnumField>("Mode");
            modeDropdown.Init(mode);
            modeDropdown.RegisterValueChangedCallback(evt => { SetMode((ViewerMode)evt.newValue); });

            renderQueueField = root.Q<SliderInt>("RenderQueue");
            renderQueueField.RegisterValueChangedCallback(evt => { renderQueue = evt.newValue; });

            stencilRefField = root.Q<StencilRefField>("StencilRef");
            stencilRefField.RegisterValueChangedCallback(evt => { stencilRef = evt.newValue; });

            var opacityField = root.Q<Slider>("Opacity");
            opacityField.RegisterValueChangedCallback(evt => { opacity = evt.newValue; });
            
            SetMode(mode);

            return root;
        }
    }
}