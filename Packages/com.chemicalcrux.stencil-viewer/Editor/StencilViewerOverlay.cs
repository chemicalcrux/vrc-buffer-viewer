using ChemicalCrux.StencilViewer.Editor.Controls;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
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
    public class StencilViewerOverlay : Overlay
    {
        private VisualElement root;
        
        private Mesh mesh;
        private Material activeMaterial;

        private bool uiReady;
        private bool shadersReady;

        private Material stencilViewerMaterial;
        private Material stencilMatcherMaterial;

        private Material displayResultMaterial;

        private StencilViewerData data;

        private static readonly int StencilRef = Shader.PropertyToID("_StencilRef");
        private static readonly int Opacity = Shader.PropertyToID("_Opacity");

        private StencilRefField stencilRefField;

        void Draw(Camera camera)
        {
            if (!displayed)
                return;

            if (!data.active)
                return;

            if (!(mesh && activeMaterial))
                return;

            if (!shadersReady)
                return;
            
            activeMaterial.renderQueue = data.renderQueue;

            activeMaterial.SetInteger(StencilRef, data.stencilRef);

            displayResultMaterial.SetFloat(Opacity, data.opacity);

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

            data = StencilViewerData.instance;
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

        private void LoadShaders()
        {
            var viewShader = Shader.Find("Hidden/chemicalcrux/Stencil Viewer/Stencil View");
            var matchShader = Shader.Find("Hidden/chemicalcrux/Stencil Viewer/Stencil Match");
            var displayShader = Shader.Find("Hidden/chemicalcrux/Stencil Viewer/Display Result");

            if (!viewShader || !matchShader || !displayShader)
                return;
            
            stencilViewerMaterial = new Material(viewShader);
            stencilMatcherMaterial = new Material(matchShader);

            displayResultMaterial = new Material(displayShader);

            shadersReady = true;
        }

        private void SetMode()
        {
            stencilRefField.style.display = DisplayStyle.None;

            switch (data.mode)
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
            uiReady = false;
            root = new VisualElement();
            TryAgain();
            return root;
        }

        private void SetupUI()
        {
            var uxml = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.chemicalcrux.stencil-viewer/UI/Overlay.uxml");

            if (uxml == null)
                return;

            uxml.CloneTree(root);

            var so = new SerializedObject(data);
            root.Bind(so);

            stencilRefField = root.Q<StencilRefField>("StencilRef");

            root.TrackPropertyValue(so.FindProperty("mode"), evt =>
            {
                SetMode();
            });

            SetMode();
            
            uiReady = true;
        }

        void TryAgain()
        {
            if (!shadersReady)
                LoadShaders();

            if (!uiReady)
                SetupUI();

            if (!shadersReady || !uiReady)
                EditorApplication.update += TryAgain;
        }
    }
}
