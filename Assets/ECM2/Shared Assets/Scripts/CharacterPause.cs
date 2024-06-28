using UnityEngine;

namespace ECM2.Examples
{
    /// <summary>
    /// Shows how to use the new (introduced in v1.4) Character Pause method to pause / resume a Character.
    /// When paused, a Character prevents any interaction.
    /// </summary>
    
    public class CharacterPause : MonoBehaviour
    {
        private Character _character;

        private void Awake()
        {
            _character = GetComponent<Character>();
        }

        private void Update()
        {
            // Toggle Character pause
            
            if (Input.GetKeyDown(KeyCode.P))
                _character.Pause(!_character.isPaused);
        }
    }
}