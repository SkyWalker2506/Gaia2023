using System.Collections.Generic;
using PWCommon5;
using UnityEngine;
using UnityEditor;
namespace ProceduralWorlds.GTS
{
    public static class PWStyles
    {
        #region Const
        private const string kStandardSpritePath = "UI/Skin/UISprite.psd";
        private const string kBackgroundSpriteResourcePath = "UI/Skin/Background.psd";
        private const string kInputFieldBackgroundPath = "UI/Skin/InputFieldBackground.psd";
        private const string kKnobPath = "UI/Skin/Knob.psd";
        private const string kCheckmarkPath = "UI/Skin/Checkmark.psd";
        #endregion
        #region Static
        private static Color SPLINE_CURVE_COLOR = new Color(0.8f, 0.8f, 0.8f);
        private static Color CURVE_BUTTON_COLOR = new Color(0.8f, 0.8f, 0.8f);
        private static Color EXTRUSION_CURVE_COLOR = new Color(0.8f, 0.8f, 0.8f);
        private static Color DIRECTION_BUTTON_COLOR = Color.red;
        private static Color UP_BUTTON_COLOR = Color.green;
        #endregion
        #region Variables
        public static readonly Texture2D knobTexture2D;
        public static readonly GUIStyle gpanel;
        public static readonly GUIStyle wrappedText;
        public static readonly GUIStyle richText;
        public static readonly GUIStyle resFlagsPanel;
        public static readonly GUIStyle resTreeFoldout;
        public static readonly GUIStyle staticResHeader;
        public static readonly GUIStyle dynamicResHeader;
        public static readonly GUIStyle boldLabel;
        public static readonly GUIStyle advancedToggle;
        public static readonly GUIStyle advancedToggleDown;
        public static readonly GUIStyle helpNoWrap;
        public static readonly GUIStyle boxLabelLeft;
        public static readonly GUIStyle boxWithLeftLabel;
        public static readonly GUIStyle addBtn;
        public static readonly GUIStyle inlineToggleBtn;
        public static readonly GUIStyle inlineToggleBtnDown;
        public static readonly GUIStyle areaDebug;
        public static readonly GUIStyle toggleBtn;
        public static readonly Texture2D undoIco;
        public static readonly GUIStyle nodeBtn;
        public static readonly GUIStyle cancelBtn;
        public static readonly GUIStyle dirBtn;
        public static readonly GUIStyle upBtn;
        public static readonly GUIStyle redBackground;
        public static readonly GUIContent redCircleContent = new GUIContent();
        public static readonly GUIContent greenCircleContent = new GUIContent();
        public static readonly GUIContent yellowCircleContent = new GUIContent();
        #region Textures
        static Texture2D m_WhiteTexture;
        /// <summary>
        /// A 1x1 white texture.
        /// </summary>
        /// <remarks>
        /// This texture is only created once and recycled afterward. You shouldn't modify it.
        /// </remarks>
        public static Texture2D whiteTexture
        {
            get
            {
                if (m_WhiteTexture == null)
                {
                    m_WhiteTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false) { name = "White Texture" };
                    m_WhiteTexture.SetPixel(0, 0, Color.white);
                    m_WhiteTexture.Apply();
                }
                return m_WhiteTexture;
            }
        }
        static Texture3D m_WhiteTexture3D;
        /// <summary>
        /// A 1x1x1 white texture.
        /// </summary>
        /// <remarks>
        /// This texture is only created once and recycled afterward. You shouldn't modify it.
        /// </remarks>
        public static Texture3D whiteTexture3D
        {
            get
            {
                if (m_WhiteTexture3D == null)
                {
                    m_WhiteTexture3D = new Texture3D(1, 1, 1, TextureFormat.ARGB32, false) { name = "White Texture 3D" };
                    m_WhiteTexture3D.SetPixels(new Color[] { Color.white });
                    m_WhiteTexture3D.Apply();
                }
                return m_WhiteTexture3D;
            }
        }
        static Texture2D m_BlackTexture;
        /// <summary>
        /// A 1x1 black texture.
        /// </summary>
        /// <remarks>
        /// This texture is only created once and recycled afterward. You shouldn't modify it.
        /// </remarks>
        public static Texture2D blackTexture
        {
            get
            {
                if (m_BlackTexture == null)
                {
                    m_BlackTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false) { name = "Black Texture" };
                    m_BlackTexture.SetPixel(0, 0, Color.black);
                    m_BlackTexture.Apply();
                }
                return m_BlackTexture;
            }
        }
        static Texture3D m_BlackTexture3D;
        /// <summary>
        /// A 1x1x1 black texture.
        /// </summary>
        /// <remarks>
        /// This texture is only created once and recycled afterward. You shouldn't modify it.
        /// </remarks>
        public static Texture3D blackTexture3D
        {
            get
            {
                if (m_BlackTexture3D == null)
                {
                    m_BlackTexture3D = new Texture3D(1, 1, 1, TextureFormat.ARGB32, false) { name = "Black Texture 3D" };
                    m_BlackTexture3D.SetPixels(new Color[] { Color.black });
                    m_BlackTexture3D.Apply();
                }
                return m_BlackTexture3D;
            }
        }
        static Texture2D m_TransparentTexture;
        /// <summary>
        /// A 1x1 transparent texture.
        /// </summary>
        /// <remarks>
        /// This texture is only created once and recycled afterward. You shouldn't modify it.
        /// </remarks>
        public static Texture2D transparentTexture
        {
            get
            {
                if (m_TransparentTexture == null)
                {
                    m_TransparentTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false) { name = "Transparent Texture" };
                    m_TransparentTexture.SetPixel(0, 0, Color.clear);
                    m_TransparentTexture.Apply();
                }
                return m_TransparentTexture;
            }
        }
        static Texture3D m_TransparentTexture3D;
        /// <summary>
        /// A 1x1x1 transparent texture.
        /// </summary>
        /// <remarks>
        /// This texture is only created once and recycled afterward. You shouldn't modify it.
        /// </remarks>
        public static Texture3D transparentTexture3D
        {
            get
            {
                if (m_TransparentTexture3D == null)
                {
                    m_TransparentTexture3D = new Texture3D(1, 1, 1, TextureFormat.ARGB32, false) { name = "Transparent Texture 3D" };
                    m_TransparentTexture3D.SetPixels(new Color[] { Color.clear });
                    m_TransparentTexture3D.Apply();
                }
                return m_TransparentTexture3D;
            }
        }
        #endregion
        /// <summary>
        /// Style for the override checkbox.
        /// </summary>
        public static readonly GUIStyle smallTickbox;
        /// <summary>
        /// Style for the labels in the toolbar of each effect.
        /// </summary>
        public static readonly GUIStyle miniLabelButton;
        static readonly Color splitterDark;
        static readonly Color splitterLight;
        /// <summary>
        /// Color of UI splitters.
        /// </summary>
        public static Color splitter => EditorGUIUtility.isProSkin ? splitterDark : splitterLight;
        static readonly Texture2D paneOptionsIconDark;
        static readonly Texture2D paneOptionsIconLight;
        /// <summary>
        /// Option icon used in effect headers.
        /// </summary>
        public static Texture2D paneOptionsIcon => EditorGUIUtility.isProSkin ? paneOptionsIconDark : paneOptionsIconLight;
        /// <summary>
        /// Style for effect header labels.
        /// </summary>
        public static readonly GUIStyle headerLabel;
        static readonly Color headerBackgroundDark;
        static readonly Color headerBackgroundLight;
        /// <summary>
        /// Color of effect header backgrounds.
        /// </summary>
        public static Color headerBackground => EditorGUIUtility.isProSkin ? headerBackgroundDark : headerBackgroundLight;
        /// <summary>
        /// Style for the trackball labels.
        /// </summary>
        public static readonly GUIStyle wheelLabel;
        /// <summary>
        /// Style for the trackball cursors.
        /// </summary>
        public static readonly GUIStyle wheelThumb;
        /// <summary>
        /// Size of the trackball cursors.
        /// </summary>
        public static readonly Vector2 wheelThumbSize;
        /// <summary>
        /// Style for the curve editor position info.
        /// </summary>
        public static readonly GUIStyle preLabel;
        private static List<Texture2D> m_texturesInMemory = new List<Texture2D>();
        #endregion
        public static float currentHalfWidth => EditorGUIUtility.currentViewWidth * 0.5f;
        public static float currentFullWidth => EditorGUIUtility.currentViewWidth;
        public static GUILayoutOption maxHalfWidth => GUILayout.MaxWidth(currentHalfWidth);
        public static GUILayoutOption maxFullWidth => GUILayout.MaxWidth(currentFullWidth);
        public static GUILayoutOption minButtonHeight => GUILayout.MinHeight(30f);
        public static Color GetColorFromHTML(string htmlString)
        {
            Color color = Color.white;
            if (!htmlString.StartsWith("#"))
                htmlString = "#" + htmlString;
            if (!ColorUtility.TryParseHtmlString(htmlString, out color))
                color = Color.white;
            return color;
        }
        public static Texture2D GetBGTexture(Color backgroundColor)
        {
            int num = 1;
            Color[] colors = new Color[num * num];
            for (int index = 0; index < colors.Length; ++index)
                colors[index] = backgroundColor;
            Texture2D bgTexture = new Texture2D(num, num);
            bgTexture.SetPixels(colors);
            bgTexture.Apply(true);
            m_texturesInMemory.Add(bgTexture);
            return bgTexture;
        }
        public static Texture2D GetBGTexture(Color backgroundColor, Color borderColor)
        {
            int num = 6;
            Color[] colors = new Color[num * num];
            for (int index1 = 0; index1 < num; ++index1)
            {
                for (int index2 = 0; index2 < num; ++index2)
                {
                    int index3 = index1 * num + index2;
                    colors[index3] = index1 == 0 || index1 == num - 1 || index2 == 0 || index2 == num - 1 ? borderColor : backgroundColor;
                }
            }
            Texture2D bgTexture = new Texture2D(num, num);
            bgTexture.SetPixels(colors);
            bgTexture.Apply(true);
            m_texturesInMemory.Add(bgTexture);
            return bgTexture;
        }
        #region Constructors
        static PWStyles()
        {
            EditorUtils.CommonStyles styles = new EditorUtils.CommonStyles();
            #region Toggle Btn
            toggleBtn = new GUIStyle(GUI.skin.toggle);
            toggleBtn.fixedWidth = 15;
            toggleBtn.margin = new RectOffset(5, 0, 0, 0);
            toggleBtn.padding = new RectOffset(0, 0, 0, 5);
            #endregion
            #region Area Debug
            areaDebug = new GUIStyle("label");
            areaDebug.normal.background = GetBGTexture(GetColorFromHTML("#ff000055"));
            #endregion
            #region Box
            gpanel = new GUIStyle(GUI.skin.box);
            gpanel.normal.textColor = GUI.skin.label.normal.textColor;
            gpanel.fontStyle = FontStyle.Bold;
            gpanel.alignment = TextAnchor.UpperLeft;
            boxLabelLeft = new GUIStyle(gpanel);
            boxLabelLeft.richText = true;
            boxLabelLeft.wordWrap = false;
            boxLabelLeft.margin.right = 0;
            boxLabelLeft.overflow.right = 1;
            boxWithLeftLabel = new GUIStyle(gpanel);
            boxWithLeftLabel.richText = true;
            boxWithLeftLabel.wordWrap = false;
            boxWithLeftLabel.margin.left = 0;
            #endregion
            #region Add Button
            addBtn = new GUIStyle("button");
            addBtn.margin = new RectOffset(4, 4, 0, 0);
            #endregion
            #region Inline Toggle Button
            inlineToggleBtn = new GUIStyle(styles.toggleButton);
            inlineToggleBtn.margin = styles.deleteButton.margin;
            inlineToggleBtnDown = new GUIStyle(styles.toggleButtonDown);
            inlineToggleBtnDown.margin = inlineToggleBtn.margin;
            #endregion
            #region Resource Tree
            resTreeFoldout = new GUIStyle(EditorStyles.foldout);
            resTreeFoldout.fontStyle = FontStyle.Bold;
            #endregion
            #region Red Flags Panel
            resFlagsPanel = new GUIStyle(GUI.skin.window);
            resFlagsPanel.normal.textColor = GUI.skin.label.normal.textColor;
            //resFlagsPanel.fontStyle = FontStyle.Bold;
            resFlagsPanel.alignment = TextAnchor.UpperCenter;
            resFlagsPanel.margin = new RectOffset(0, 0, 5, 7);
            resFlagsPanel.padding = new RectOffset(10, 10, 3, 3);
            resFlagsPanel.stretchWidth = true;
            resFlagsPanel.stretchHeight = false;
            #endregion
            #region Wrap Style
            wrappedText = new GUIStyle(GUI.skin.label);
            wrappedText.fontStyle = FontStyle.Normal;
            wrappedText.wordWrap = true;
            #endregion
            #region Rich Text
            richText = new GUIStyle(GUI.skin.label);
            richText.fontStyle = FontStyle.Normal;
            richText.wordWrap = true;
            richText.richText = true;
            #endregion
            #region Static / Dynamic Resource Header
            staticResHeader = new GUIStyle();
            staticResHeader.overflow = new RectOffset(2, 2, 2, 2);
            dynamicResHeader = new GUIStyle(staticResHeader);
            #endregion
            #region Bold Label
            boldLabel = new GUIStyle("Label");
            boldLabel.fontStyle = FontStyle.Bold;
            #endregion
            #region Advanced Toggle
            advancedToggle = styles.toggleButton;
            advancedToggle.padding = new RectOffset(5, 5, 0, 0);
            advancedToggle.margin = styles.deleteButton.margin;
            advancedToggle.fixedHeight = styles.deleteButton.fixedHeight;
            advancedToggleDown = styles.toggleButtonDown;
            advancedToggleDown.padding = advancedToggle.padding;
            advancedToggleDown.margin = advancedToggle.margin;
            advancedToggleDown.fixedHeight = advancedToggle.fixedHeight;
            #endregion
            #region Help
            helpNoWrap = new GUIStyle(styles.help);
            helpNoWrap.wordWrap = false;
            #endregion
            #region Undo Icon
            undoIco = Resources.Load("pwundo" + PWConst.VERSION_IN_FILENAMES) as Texture2D;
            #endregion
            #region Background
            redBackground = new GUIStyle();
            redBackground.active.background = GetBGTexture(Color.red);
            #endregion
            #region Spline Buttons
            knobTexture2D = AssetDatabase.GetBuiltinExtraResource<Texture2D>(kKnobPath);
            nodeBtn = new GUIStyle("button");
            nodeBtn.active.background = GetBGTexture(Color.white);
            dirBtn = new GUIStyle("button");
            dirBtn.active.background = knobTexture2D; // GetBGTexture(Color.white); 
            dirBtn.normal.background = knobTexture2D;
            upBtn = new GUIStyle("button");
            upBtn.active.background = GetBGTexture(Color.white);
            #endregion
            #region Cancel Button
            cancelBtn = new GUIStyle("button");
            //cancelBtn.normal.textColor = Color.red;
            #endregion
            #region Unity Personal / Pro Colors
            // Setup colors for Unity Pro
            if (EditorGUIUtility.isProSkin)
            {
                resFlagsPanel.normal.background = Resources.Load("pwdarkBoxp" + PWConst.VERSION_IN_FILENAMES) as Texture2D;
                staticResHeader.normal.background = GetBGTexture(GetColorFromHTML("2d2d2dff"));
                dynamicResHeader.normal.background = GetBGTexture(GetColorFromHTML("2d2d4cff"));
            }
            // or Unity Personal
            else
            {
                resFlagsPanel.normal.background = Resources.Load("pwdarkBox" + PWConst.VERSION_IN_FILENAMES) as Texture2D;
                staticResHeader.normal.background = GetBGTexture(GetColorFromHTML("a2a2a2ff"));
                dynamicResHeader.normal.background = GetBGTexture(GetColorFromHTML("a2a2c1ff"));
            }
            #endregion
            #region Styling
            smallTickbox = new GUIStyle("ShurikenToggle");
            miniLabelButton = new GUIStyle(EditorStyles.miniLabel);
            miniLabelButton.normal = new GUIStyleState
            {
                background = transparentTexture,
                scaledBackgrounds = null,
                textColor = Color.grey
            };
            var activeState = new GUIStyleState
            {
                background = transparentTexture,
                scaledBackgrounds = null,
                textColor = Color.white
            };
            miniLabelButton.active = activeState;
            miniLabelButton.onNormal = activeState;
            miniLabelButton.onActive = activeState;
            splitterDark = new Color(0.12f, 0.12f, 0.12f, 1.333f);
            splitterLight = new Color(0.6f, 0.6f, 0.6f, 1.333f);
            headerBackgroundDark = new Color(0.1f, 0.1f, 0.1f, 0.2f);
            headerBackgroundLight = new Color(1f, 1f, 1f, 0.2f);
            paneOptionsIconDark = (Texture2D)EditorGUIUtility.Load("Builtin Skins/DarkSkin/Images/pane options.png");
            paneOptionsIconLight = (Texture2D)EditorGUIUtility.Load("Builtin Skins/LightSkin/Images/pane options.png");
            headerLabel = new GUIStyle(EditorStyles.miniLabel);
            wheelThumb = new GUIStyle("ColorPicker2DThumb");
            wheelThumbSize = new Vector2(
                !Mathf.Approximately(wheelThumb.fixedWidth, 0f) ? wheelThumb.fixedWidth : wheelThumb.padding.horizontal,
                !Mathf.Approximately(wheelThumb.fixedHeight, 0f) ? wheelThumb.fixedHeight : wheelThumb.padding.vertical
            );
            wheelLabel = new GUIStyle(EditorStyles.miniLabel);
            preLabel = new GUIStyle("ShurikenLabel");
            #endregion
            #region GUI Content
            redCircleContent = EditorGUIUtility.IconContent("lightMeter/redLight");
            greenCircleContent = EditorGUIUtility.IconContent("lightMeter/greenLight");
            yellowCircleContent = EditorGUIUtility.IconContent("lightMeter/orangeLight");
            #endregion
        }
        #endregion
    }
}