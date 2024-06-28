using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

namespace Akila.MasterFPSCounter
{
    public class MasterFPSCounterWizards : Editor
    {
        /// <summary>
        /// Creates game object with default information counters (FPS, AVG FPS, etc.).
        /// </summary>
        [MenuItem("GameObject/Akila/Master FPS Counter/Info Container (FPS Counter)")]
        public static void CreateInfoContainer()
        {
            Canvas canvas = MasterFPSCounterEditor.FindOrCreateCanves(true);

            GameObject infocontainerGameObject = new GameObject("Info Container (FPS Counter)");
            RectTransform infocontainerRectTransform = infocontainerGameObject.AddComponent<RectTransform>();
            infocontainerRectTransform.transform.SetParent(canvas.transform);
            infocontainerRectTransform.anchoredPosition = Vector3.zero;
            infocontainerRectTransform.sizeDelta = Vector2.zero;

            infocontainerRectTransform.anchorMax = new Vector2(0, 1);
            infocontainerRectTransform.anchorMin = new Vector2(0, 1);

            InfoContainer infoContainer = infocontainerGameObject.AddComponent<InfoContainer>();
            HorizontalLayoutGroup horizontalLayoutGroup = infocontainerGameObject.AddComponent<HorizontalLayoutGroup>();
            horizontalLayoutGroup.padding.left = 15;
            horizontalLayoutGroup.spacing = 8;

            TextMeshProUGUI fps = CreateInfoContainerItem("<color=#808080>FPS: <color=#FFFFFF>60", "FPS", infocontainerGameObject.transform);
            TextMeshProUGUI avgFPS = CreateInfoContainerItem("<color=#808080>AVG FPS: <color=#FFFFFF>60", "AVG FPS", infocontainerGameObject.transform);
            TextMeshProUGUI maxFPS = CreateInfoContainerItem("<color=#808080>Max FPS: <color=#FFFFFF>60", "Max FPS", infocontainerGameObject.transform);
            TextMeshProUGUI minFPS = CreateInfoContainerItem("<color=#808080>Min FPS: <color=#FFFFFF>60", "Min FPS", infocontainerGameObject.transform);

            infoContainer.fpsText = fps;
            infoContainer.averageFPSText = avgFPS;
            infoContainer.maxFPSText = maxFPS;
            infoContainer.minFPSText = minFPS;

            Selection.SetActiveObjectWithContext(infocontainerGameObject, infocontainerGameObject);
            Undo.RegisterCreatedObjectUndo(infocontainerGameObject, "Created an info container");
            MasterFPSCounterEditor.PrintDefaultMessage("Created an info container");
        }

        /// <summary>
        /// Creates simple text with basic FPS Counter component.
        /// </summary>
        [MenuItem("GameObject/Akila/Master FPS Counter/Basic FPS Counter")]
        public static void CreateBasicFPSCounter()
        {
            GameObject basicFPSCounterGameObject = new GameObject("Basic FPS Counter");
            TextMeshProUGUI textMeshProUGUI = basicFPSCounterGameObject.AddComponent<TextMeshProUGUI>();
            BasicFPSCounter counter = textMeshProUGUI.gameObject.AddComponent<BasicFPSCounter>();
            textMeshProUGUI.transform.SetParent(MasterFPSCounterEditor.FindOrCreateCanves(true).transform);

            textMeshProUGUI.rectTransform.sizeDelta = new Vector2(300, 50);
            textMeshProUGUI.rectTransform.anchoredPosition = Vector3.zero;
            textMeshProUGUI.rectTransform.rotation = Quaternion.identity;
            textMeshProUGUI.rectTransform.localScale = Vector3.one;

            textMeshProUGUI.alignment = TextAlignmentOptions.Center;
            textMeshProUGUI.text = "<color=#808080>FPS: <color=#FFFFFF>60";

            Selection.SetActiveObjectWithContext(basicFPSCounterGameObject, basicFPSCounterGameObject);
            Undo.RegisterCreatedObjectUndo(basicFPSCounterGameObject, "Created basic FPS counter");
            MasterFPSCounterEditor.PrintDefaultMessage("Created a basic FPS Counter");
        }

        public static TextMeshProUGUI CreateInfoContainerItem(string defaultText, string textName, Transform parent)
        {
            Image itemImage = new GameObject(textName).AddComponent<Image>();
            TextMeshProUGUI itemText = new GameObject("Text (TMP)").AddComponent<TextMeshProUGUI>();
            HorizontalLayoutGroup itemHorizontalGroup = itemImage.AddComponent<HorizontalLayoutGroup>();
            ContentSizeFitter itemSizeFitter = itemImage.AddComponent<ContentSizeFitter>();
            ContentSizeFitter itemTextsSizeFitter = itemText.AddComponent<ContentSizeFitter>();
            itemImage.color = Color.black * 0.7f;

            itemImage.transform.SetParent(parent);
            itemImage.rectTransform.anchoredPosition = Vector3.zero;
            itemImage.rectTransform.sizeDelta = new Vector2(90, 32);
            itemText.rectTransform.SetParent(itemImage.transform);
            itemSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            itemSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            itemTextsSizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            itemTextsSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            itemHorizontalGroup.padding.bottom = 5;
            itemHorizontalGroup.padding.top = 5;
            itemHorizontalGroup.padding.right = 12;
            itemHorizontalGroup.padding.left = 12;
            itemText.fontSize = 25;
            itemText.text = defaultText;

            return itemText;
        }
    }
}