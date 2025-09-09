using System;
using System.Collections.Generic;
using System.Linq;
using Crux.BufferViewer.Editor.Controls;
using Crux.Core.Editor;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Crux.BufferViewer.Editor
{
    public enum ViewerMode
    {
        StencilBuffer,
        StencilTest,
        DepthBuffer
    }

    [Overlay(typeof(SceneView), "Buffer Viewer")]
    public class BufferViewerOverlay : Overlay
    {
        private VisualElement root;
        
        private Mesh mesh;
        private Material activeMaterial;

        private bool uiReady;
        private bool shadersReady;

        private Material stencilViewerMaterial;
        private Material stencilMatcherMaterial;
        private Material depthViewerMaterial;

        private Material displayResultMaterial;

        private BufferViewerData data;

        private static readonly int StencilRef = Shader.PropertyToID("_StencilRef");
        private static readonly int StencilComp = Shader.PropertyToID("_StencilComp");
        private static readonly int ShowFarPlane = Shader.PropertyToID("_ShowFarPlane");
        
        private static readonly int Opacity = Shader.PropertyToID("_Opacity");
        private static readonly int BlendMode = Shader.PropertyToID("_BlendMode");

        private StencilRefField stencilRefField;
        private RenderQueueSlider renderQueueSlider;

        private VisualElement stencilSettingsHolder;
        private VisualElement stencilTestSettingsHolder;
        private VisualElement depthSettingsHolder;

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
            activeMaterial.SetFloat(StencilComp, (int) data.stencilComp);
            activeMaterial.SetFloat(ShowFarPlane, data.showFarPlane ? 1 : 0);

            displayResultMaterial.SetFloat(Opacity, data.opacity);
            displayResultMaterial.SetFloat(BlendMode, (float) data.blendMode);

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

            data = BufferViewerData.instance;
        }

        public override void OnWillBeDestroyed()
        {
            base.OnWillBeDestroyed();

            Object.DestroyImmediate(displayResultMaterial);

            Object.DestroyImmediate(stencilViewerMaterial);
            Object.DestroyImmediate(stencilMatcherMaterial);
            Object.DestroyImmediate(depthViewerMaterial);

            Object.DestroyImmediate(mesh);

            Camera.onPreCull -= Draw;
        }

        private void LoadShaders()
        {
            var viewShader = Shader.Find("Hidden/chemicalcrux/Buffer Viewer/Stencil View");
            var matchShader = Shader.Find("Hidden/chemicalcrux/Buffer Viewer/Stencil Match");
            var depthShader = Shader.Find("Hidden/chemicalcrux/Buffer Viewer/Depth View");
            var displayShader = Shader.Find("Hidden/chemicalcrux/Buffer Viewer/Display Result");

            if (!viewShader || !matchShader || !displayShader)
                return;
            
            stencilViewerMaterial = new Material(viewShader);
            stencilMatcherMaterial = new Material(matchShader);
            depthViewerMaterial = new Material(depthShader);

            displayResultMaterial = new Material(displayShader);

            shadersReady = true;
        }

        private void SetMode()
        {
            stencilRefField.style.display = DisplayStyle.None;
            stencilSettingsHolder.style.display = DisplayStyle.None;
            stencilTestSettingsHolder.style.display = DisplayStyle.None;
            depthSettingsHolder.style.display = DisplayStyle.None;

            switch (data.mode)
            {
                case ViewerMode.StencilBuffer:
                    stencilSettingsHolder.style.display = DisplayStyle.Flex;
                    activeMaterial = stencilViewerMaterial;
                    break;
                case ViewerMode.StencilTest:
                    stencilSettingsHolder.style.display = DisplayStyle.Flex;
                    stencilTestSettingsHolder.style.display = DisplayStyle.Flex;
                    stencilRefField.style.display = DisplayStyle.Flex;
                    activeMaterial = stencilMatcherMaterial;
                    break;
                case ViewerMode.DepthBuffer:
                    depthSettingsHolder.style.display = DisplayStyle.Flex;
                    activeMaterial = depthViewerMaterial;
                    break;
                default:
                    Debug.LogWarning("I don't know how you picked an invalid mode");
                    break;
            }

            if (uiReady && renderQueueSlider != null)
            {
                foreach (var group in GetQueuesInScene())
                {
                    renderQueueSlider.AddTick(group.Item1);
                }
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
            if (!AssetReference.TryParse("96a0258b89adc440e85f879eed057739,9197481963319205126", out var assetRef))
                return;

            if (!assetRef.TryLoad(out VisualTreeAsset uxml))
                return;
            
            uxml.CloneTree(root);

            var so = new SerializedObject(data);
            root.Bind(so);

            stencilRefField = root.Q<StencilRefField>("StencilRef");
            stencilSettingsHolder = root.Q("StencilSettings");
            stencilTestSettingsHolder = root.Q("StencilTestSettings");
            depthSettingsHolder = root.Q("DepthSettings");

            root.TrackPropertyValue(so.FindProperty("mode"), _ =>
            {
                SetMode();
            });

            uiReady = true;
            
            EditorApplication.delayCall += WaitForUI;
        }

        void TryAgain()
        {
            if (!shadersReady)
                LoadShaders();

            if (!uiReady)
                SetupUI();

            if (!shadersReady || !uiReady)
                EditorApplication.delayCall += TryAgain;
        }

        void WaitForUI()
        {
            renderQueueSlider = root.Q<RenderQueueSlider>();
            SetMode();
        }

        private IEnumerable<(int, int)> GetQueuesInScene()
        {
            return Object.FindObjectsByType<Renderer>(FindObjectsSortMode.InstanceID)
                .SelectMany(static renderer => renderer.sharedMaterials)
                .GroupBy(static material => material.renderQueue)
                .OrderBy(static group => group.Key)
                .Select(static group => (group.Key, group.Count()));
        }

        public IEnumerable<(int, IEnumerable<(Renderer, Material)>)> GetAllRendererMaterialPairs()
        {
            Vector3 x = default;
            float y = 1;
            Math.Abs(y);
            
            return Object.FindObjectsByType<Renderer>(FindObjectsSortMode.InstanceID)
                .SelectMany(static renderer => renderer.sharedMaterials
                    .Select(material => (renderer, material)))
                .GroupBy(tuple => tuple.material.renderQueue)
                .OrderBy(grouping => grouping.Key)
                .Select(grouping => (grouping.Key, grouping.AsEnumerable()));
        }
    }
}
