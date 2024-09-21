using UnityEngine;

namespace MajdataPlay.Misc
{
    public class DestroySelf : MonoBehaviour
    {
        public bool ifDestroy;

        private void Update()
        {
            if (ifDestroy) Destroy(gameObject);
        }
    }
}