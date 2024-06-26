
using UnityEngine;
#if HDPipeline || URP
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
#endif

[System.Serializable]
public class DepthOfFieldParameters
{
    public float minDistance = 1f;
    public float maxDistance = 50;
    public float focusSpeed = 40f;
    public float minNearRangeStart = 0f;
    public float minNearRangeEnd = 0.5f;
    public float minFarRangeStart = 20f;
    public float minFarRangeEnd = 100f;
    public float maxNearRangeStart = 0f;
    public float maxNearRangeEnd = 2f;
    public float maxFarRangeStart = 2000f;
    public float maxFarRangeEnd = 5000f;
}

public class DynamicDepthOfField : MonoBehaviour
{
    public LayerMask raycastLayerMask = -1;

    [Header("Depth of Field Parameters")]
    public DepthOfFieldParameters parameters;

    // Default values
    private DepthOfFieldParameters defaultParameters;

    private float currentNearRangeStart;
    private float currentNearRangeEnd;
    private float currentFarRangeStart;
    private float currentFarRangeEnd;
    private float targetNearRangeStart;
    private float targetNearRangeEnd;
    private float targetFarRangeStart;
    private float targetFarRangeEnd;

    private float normalizedRangeDistance = 1f;

    public bool showGizmo = false;
    public Color gizmoColor = Color.red;

    public Camera mainCamera;
    private Vector3 hitPoint;

#if HDPipeline || URP
    private Volume volumeComponent;
    private DepthOfField depthOfField;
#endif

    private void Start()
    {
        // Check if a camera is assigned
        if (mainCamera == null)
        {
            // If not, try to find the active camera
            mainCamera = Camera.main;

            // If still no camera, throw an error and skip the rest of the code
            if (mainCamera == null)
            {
                Debug.LogError("No camera assigned or found!");
                return;
            }
        }

#if HDPipeline || URP
        volumeComponent = GetComponent<Volume>();
        if (volumeComponent == null)
        {
            volumeComponent = gameObject.AddComponent<Volume>();
        }

        if (!volumeComponent.profile.TryGet(out depthOfField))
        {
            depthOfField = volumeComponent.profile.Add<DepthOfField>();
        }
#endif
        // Save default values
        defaultParameters = new DepthOfFieldParameters();
        defaultParameters.minDistance = parameters.minDistance;
        defaultParameters.maxDistance = parameters.maxDistance;
        defaultParameters.focusSpeed = parameters.focusSpeed;
        defaultParameters.minFarRangeStart = parameters.minFarRangeStart;
        defaultParameters.maxFarRangeStart = parameters.maxFarRangeStart;
        defaultParameters.minFarRangeEnd = parameters.minFarRangeEnd;
        defaultParameters.maxFarRangeEnd = parameters.maxFarRangeEnd;
        defaultParameters.minNearRangeStart = parameters.minNearRangeStart;
        defaultParameters.maxNearRangeStart = parameters.maxNearRangeStart;
        defaultParameters.minNearRangeEnd = parameters.minNearRangeEnd;
        defaultParameters.maxNearRangeEnd = parameters.maxNearRangeEnd;
        CheckAndSetupDepthOfField();
    }

    private void Update()
    {
        UpdateDepthOfFieldFocus();
    }

    private void CheckAndSetupDepthOfField()
    {
#if HDPipeline || URP
        // Enable Depth of Field
        depthOfField.active = true;

        // Enable other necessary Depth of Field settings
        depthOfField.focusMode.overrideState = true;
        depthOfField.focusMode.value = DepthOfFieldMode.Manual;

        // Adjust the near and far manual ranges
        depthOfField.nearFocusStart.overrideState = true;
        depthOfField.farFocusStart.overrideState = true;
        depthOfField.nearFocusEnd.overrideState = true;
        depthOfField.farFocusEnd.overrideState = true;
#endif
    }

    private void UpdateDepthOfFieldFocus()
    {
        if (mainCamera == null)
        {
            return;
        }
       
        normalizedRangeDistance = 1f;

        Ray centerRay = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
        RaycastHit centerHit;

        // Start the raycast
        if (Physics.Raycast(centerRay, out centerHit, parameters.maxDistance, raycastLayerMask))
        {
            hitPoint = centerHit.point;

            normalizedRangeDistance = Mathf.InverseLerp(parameters.minDistance, parameters.maxDistance, centerHit.distance);

        }

        targetNearRangeStart = Mathf.Lerp(parameters.minNearRangeStart, parameters.maxNearRangeStart, normalizedRangeDistance);
        targetNearRangeEnd = Mathf.Lerp(parameters.minNearRangeEnd, parameters.maxNearRangeEnd, normalizedRangeDistance);
        targetFarRangeStart = Mathf.Lerp(parameters.minFarRangeStart, parameters.maxFarRangeStart, normalizedRangeDistance);
        targetFarRangeEnd = Mathf.Lerp(parameters.minFarRangeEnd, parameters.maxFarRangeEnd, normalizedRangeDistance);

        currentNearRangeStart = Mathf.MoveTowards(currentNearRangeStart, targetNearRangeStart, Mathf.Abs(currentFarRangeStart - targetFarRangeStart) * parameters.focusSpeed * Time.deltaTime);
        currentNearRangeEnd = Mathf.MoveTowards(currentNearRangeEnd, targetNearRangeEnd, Mathf.Abs(currentNearRangeEnd - targetNearRangeEnd) * parameters.focusSpeed * Time.deltaTime);
        currentFarRangeStart = Mathf.MoveTowards(currentFarRangeStart, targetFarRangeStart, Mathf.Abs(currentFarRangeStart - targetFarRangeStart) * parameters.focusSpeed * Time.deltaTime);
        currentFarRangeEnd = Mathf.MoveTowards(currentFarRangeEnd, targetFarRangeEnd, Mathf.Abs(currentFarRangeEnd - targetFarRangeEnd) * parameters.focusSpeed * Time.deltaTime);


#if HDPipeline || URP
        // Apply the calculated focus distances
        depthOfField.nearFocusStart.value = currentNearRangeStart;
        depthOfField.nearFocusEnd.value = currentNearRangeEnd;
        depthOfField.farFocusStart.value = currentFarRangeStart;
        depthOfField.farFocusEnd.value = currentFarRangeEnd;
        
#endif
    }




    private void OnDrawGizmos()
    {
        if (showGizmo && normalizedRangeDistance<1)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(hitPoint, 0.2f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (showGizmo && normalizedRangeDistance < 1)
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(hitPoint, 0.2f);
        }
    }
    

    // Button function to restore default values
    [ContextMenu("Restore Default Values")]
    private void RestoreDefaultValues()
    {
        parameters = defaultParameters;
    }

}
