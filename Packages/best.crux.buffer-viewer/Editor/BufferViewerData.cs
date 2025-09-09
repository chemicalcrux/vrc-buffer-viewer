using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Crux.BufferViewer.Editor
{
    public enum BlendMode
    {
        Normal = 0,
        Multiply = 1
    }
    
    public class BufferViewerData : ScriptableSingleton<BufferViewerData>
    {
        public bool active;
        public ViewerMode mode;
        
        public int renderQueue = 4000;
        public int stencilRef;
        public CompareFunction stencilComp = CompareFunction.Equal;
        
        public float opacity = 1;
        public BlendMode blendMode;

        public bool showFarPlane;
        public Vector2 depthRange;
    }
}