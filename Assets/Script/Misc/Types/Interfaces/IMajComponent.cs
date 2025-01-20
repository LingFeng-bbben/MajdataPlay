using UnityEngine;
#nullable enable
namespace MajdataPlay.Types
{
    public interface IMajComponent : IGameObjectProvider , ITransformProvider
    {
        string Tag { get; }
        bool Active { get; }

        void SetActive(bool state);
    }
}
