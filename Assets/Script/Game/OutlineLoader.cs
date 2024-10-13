using MajdataPlay.Utils;
using UnityEngine;

namespace MajdataPlay.Game
{
    public class OutlineLoader : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            GetComponent<SpriteRenderer>().sprite = MajInstances.SkinManager.SelectedSkin.Outline;
        }
    }
}