using System;
using UnityEngine;

namespace Unity.VisualScripting
{
    public abstract class LudiqBehaviour : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField, DoNotSerialize] // Serialize with Unity, but not with FullSerializer.
        protected SerializationData _data;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (Serialization.isCustomSerializing)
            {
                return;
            }

            Serialization.isUnitySerializing = true;

            try
            {
                OnBeforeSerialize();
                _data = this.Serialize(true);
                OnAfterSerialize();
            }
            catch (Exception ex)
            {
                // Don't abort the whole serialization thread because this one object failed
                Debug.LogError($"Failed to serialize behaviour.\n{ex}", this);
            }

            Serialization.isUnitySerializing = false;
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (Serialization.isCustomSerializing)
            {
                return;
            }

            Serialization.isUnitySerializing = true;

            try
            {
                object @this = this;
                OnBeforeDeserialize();
                _data.DeserializeInto(ref @this, true);
                OnAfterDeserialize();
                _data.Clear();
            }
            catch (Exception ex)
            {
                // Don't abort the whole deserialization thread because this one object failed
                Debug.LogError($"Failed to deserialize behaviour.\n{ex}", this);
            }

            Serialization.isUnitySerializing = false;
        }

        protected virtual void OnBeforeSerialize() { }

        protected virtual void OnAfterSerialize() { }

        protected virtual void OnBeforeDeserialize() { }

        protected virtual void OnAfterDeserialize() { }

        protected virtual void ShowData()
        {
            var data = this.Serialize(true);
            data.ShowString(ToString());
            data.Clear();
        }

        public override string ToString()
        {
            return this.ToSafeString();
        }
    }
}
