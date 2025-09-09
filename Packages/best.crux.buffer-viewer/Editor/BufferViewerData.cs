using UnityEditor;
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
        public int stencilRef = 0;
        public CompareFunction stencilComp = CompareFunction.Equal;
        
        public float opacity = 1;
        public BlendMode blendMode;

        public bool showFarPlane;
    }
}