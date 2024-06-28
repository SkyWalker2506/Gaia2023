using UnityEngine;

namespace ECM2.Walkthrough.Ex81
{
    /// <summary>
    /// This example make use of the new (introduced in v1.4) NavMeshCharacter component,
    /// to implement a typical click to move.
    /// 
    /// The NavMeshCharacter component replaces the AgentCharacter adding NavMesh navigation
    /// capabilities to a Character through composition.
    /// </summary>
    
    public class ClickToMove : MonoBehaviour
    {
        public Camera mainCamera;
        public Character character;

        public LayerMask groundMask;

        private NavMeshCharacter _navMeshCharacter;

        private void Awake()
        {
            _navMeshCharacter = character.GetComponent<NavMeshCharacter>();
        }

        private void Update()
        {
            if (Input.GetMouseButton(0))
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hitResult, Mathf.Infinity, groundMask))
                    _navMeshCharacter.MoveToDestination(hitResult.point);
            }
        }
    }
}