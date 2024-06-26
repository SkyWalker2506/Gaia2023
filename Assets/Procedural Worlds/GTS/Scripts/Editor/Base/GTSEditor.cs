using PWCommon5;
using UnityEngine;
using UnityEditor;
namespace ProceduralWorlds.GTS
{
    public class GTSEditor : Editor, IPWEditor
    {
        #region Variables
        protected EditorUtils m_editorUtils;
        protected bool m_inited = false;
        #endregion
        #region Properties
        public Rect position { get; set; }
        public bool maximized { get; set; }
        public bool PositionChecked { get; set; }
        #endregion
        #region Methods
        protected void Initialize()
        {
            // Initialize GUI
            if (m_inited == false)
                m_inited = true;
            // Initialize Editor Utils (if it exists)
            m_editorUtils?.Initialize();
        }
        public override void OnInspectorGUI() => Initialize();
        public virtual void OnSceneGUI()
        {
        }
        public virtual void OnFocus()
        {
        }
        public virtual void OnLostFocus()
        {
        }
        #endregion
    }
}