using UnityEngine;

namespace SoundManager
{
    [CreateAssetMenu(menuName = "Sound SO", fileName = "Sounds SO")]
    public class SoundSO : ScriptableObject 
    {
        public SoundList[] sounds;
    }
}
