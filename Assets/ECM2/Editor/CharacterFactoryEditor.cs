using UnityEditor;
using UnityEngine;

namespace ECM2.Editor
{
    public static class CharacterFactoryEditor
    {
        private const string PATH = "GameObject/ECM2/";
        private const int PRIORITY = 1;

        private static void InitPhysicsBody(GameObject go)
        {
            Rigidbody rb = go.GetComponent<Rigidbody>();

            rb.drag = 0.0f;
            rb.angularDrag = 0.0f;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            CapsuleCollider capsuleCollider = go.GetComponent<CapsuleCollider>();

            capsuleCollider.center = new Vector3(0f, 1f, 0f);
            capsuleCollider.radius = 0.5f;
            capsuleCollider.height = 2.0f;
        }
        
        [MenuItem(PATH + "Character", false, PRIORITY)]
        public static void CreateCharacter()
        {
            // Create an initialize a new Character GameObject

            GameObject go = new GameObject("Character", typeof(Rigidbody), typeof(CapsuleCollider),
                typeof(CharacterMovement), typeof(Character));

            InitPhysicsBody(go);
            
            // Focus the newly created character

            Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);

            Selection.activeGameObject = go;
            SceneView.FrameLastActiveSceneView();
        }
    }
}