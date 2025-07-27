
using UnityEngine;

namespace ProjectChimera.Data.Narrative
{
    [CreateAssetMenu(fileName = "New Personality Profile", menuName = "Project Chimera/Narrative/Personality Profile")]
    public class PersonalityProfileSO : ScriptableObject
    {
        public float Loyalty;
        public float Independence;
        public float Skepticism;
        public float Strictness;
        public float Compassion;
        public float Leadership;
    }
}
