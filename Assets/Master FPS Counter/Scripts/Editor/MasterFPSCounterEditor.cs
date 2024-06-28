using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Akila.MasterFPSCounter
{
    public class MasterFPSCounterEditor : Editor
    {
        /// <summary>
        /// Creates a canves if the scene doesn't have any or all of them are disabled.
        /// </summary>
        /// <returns></returns>
        public static Canvas FindOrCreateCanves(bool includeEventSystem = false)
        {
            Canvas canves = null;
            CanvasScaler canvasScaler = null;
            GraphicRaycaster raycaster = null;

            //Add event system
            if (includeEventSystem) FindOrCreateEventSystem();

            //If there's a canves in the scene, return that canves
            if (FindObjectOfType<Canvas>())
            {
                canves = FindObjectOfType<Canvas>();
                Selection.activeTransform = canves.transform;

                return canves;
            }

            GameObject canvasObject = new GameObject("Canves");

            //Reset all transform values
            canvasObject.transform.position = Vector3.zero;
            canvasObject.transform.rotation = Quaternion.identity;
            canvasObject.transform.localScale = Vector3.one;

            //Add requied components
            canves = canvasObject.AddComponent<Canvas>();
            canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            raycaster = canvasObject.AddComponent<GraphicRaycaster>();

            //Set default values
            canves.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0;
            canvasScaler.referencePixelsPerUnit = 100;
            raycaster.ignoreReversedGraphics = true;

            //Select the canves
            Selection.activeGameObject = canvasObject.gameObject;
            Undo.RegisterCreatedObjectUndo(canvasObject, "Created canvas");
            return canves;
        }

        /// <summary>
        /// Creates an event system if the scene doesn't have any or all of them are disabled.
        /// </summary>
        /// <returns></returns>
        public static EventSystem FindOrCreateEventSystem()
        {
            EventSystem eventSystem = null;
            StandaloneInputModule standaloneInputModule = null;

            //If there's an event system in the scene, return that event system
            if (FindObjectOfType<EventSystem>())
            {
                eventSystem = FindObjectOfType<EventSystem>();
                Selection.activeTransform = eventSystem.transform;

                return eventSystem;
            }

            GameObject eventSystemObject = new GameObject("Event System");

            //Reset all transform values
            eventSystemObject.transform.position = Vector3.zero;
            eventSystemObject.transform.rotation = Quaternion.identity;
            eventSystemObject.transform.localScale = Vector3.one;

            //Add requied components
            eventSystem = eventSystemObject.AddComponent<EventSystem>();
            standaloneInputModule = eventSystemObject.AddComponent<StandaloneInputModule>();

            //Select the event system
            Selection.activeGameObject = eventSystemObject.gameObject;
            Undo.RegisterCreatedObjectUndo(eventSystemObject, "Created an event system");
            return eventSystem;
        }

        public static void PrintDefaultMessage(string additions = "")
        {
            Debug.Log($"{additions}. Don't forget to check out other products like <a href=\"https://assetstore.unity.com/packages/slug/217379\">FPS Framework</a> and <a href=\"https://assetstore.unity.com/packages/slug/217379\">leave a review</a> on this one.");
        }
    }
}