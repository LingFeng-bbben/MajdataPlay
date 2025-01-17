using UnityEngine;
#nullable enable
namespace MajdataPlay.Types
{
    public interface IMajComponent : IGameObjectProvider , ITransformProvider
    {
        public bool Active { get; }
        void SetActive(bool state);
    }
}
