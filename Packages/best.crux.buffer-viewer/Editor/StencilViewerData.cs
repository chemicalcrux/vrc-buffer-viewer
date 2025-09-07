using UnityEditor;

namespace ChemicalCrux.StencilViewer.Editor
{
    public enum BlendMode
    {
        Normal = 0,
        Multiply = 1
    }
    
    public class StencilViewerData : ScriptableSingleton<StencilViewerData>
    {
        public bool active;
        public ViewerMode mode;
        
        public int renderQueue = 4000;
        public int stencilRef = 0;
        public float opacity = 1;
        public BlendMode blendMode;

        public bool showFarPlane;
    }
}