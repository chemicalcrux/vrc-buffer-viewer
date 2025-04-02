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

        private bool activated;

        private ViewerMode mode;

        private int stencilRef;
        
        private static readonly int StencilRef = Shader.PropertyToID("_StencilRef");

        void Draw(Camera camera)
        {
            if (!displayed)
                return;

            if (!activated)
                return;
            
            if (!(mesh && activeMaterial))
                return;

            activeMaterial.SetInteger(StencilRef, stencilRef);
            RenderParams rp = new RenderParams(activeMaterial);

            Matrix4x4 mat =
                Matrix4x4.TRS(camera.transform.position + camera.transform.forward * camera.nearClipPlane * 1.1f,
                    camera.transform.rotation, camera.transform.lossyScale);

            rp.camera = camera;
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

            SetMode(mode);
        }

        public override void OnWillBeDestroyed()
        {
            base.OnWillBeDestroyed();
            
            Object.DestroyImmediate(mesh);
            
            Object.DestroyImmediate(stencilViewerMaterial);
            Object.DestroyImmediate(stencilMatcherMaterial);
            
            Camera.onPreCull -= Draw;
        }

        private void SetMode(ViewerMode newMode)
        {
            mode = newMode;

            switch (mode)
            {
                case ViewerMode.ShowBuffer:
                    activeMaterial = stencilViewerMaterial;
                    break;
                case ViewerMode.ShowMatching:
                    activeMaterial = stencilMatcherMaterial;
                    break;
                default:
                    Debug.LogWarning("I don't know how you picked an invalid mode");
                    break;
            }
        }
        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement();

            var toggle = new Toggle
            {
                label = "Active"
            };

            root.Add(toggle);

            toggle.RegisterValueChangedCallback(evt =>
            {
                activated = evt.newValue;
            });

            var modeDropdown = new EnumField();
            modeDropdown.Init(mode);

            modeDropdown.RegisterValueChangedCallback(evt =>
            {
                SetMode((ViewerMode)evt.newValue);
            });

            root.Add(modeDropdown);

            var refField = new StencilRefField
            {
                value = stencilRef
            };

            refField.RegisterValueChangedCallback(evt =>
            {
                stencilRef = evt.newValue;
            });

            root.Add(refField);
            return root;
        }
    }
}