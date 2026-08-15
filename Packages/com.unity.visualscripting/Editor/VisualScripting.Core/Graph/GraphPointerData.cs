using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting
{
    public sealed class GraphPointerData
    {
#if UNITY_6000_4_OR_NEWER
        [Serialize]
        private EntityId rootObjectInstanceID;
#else
        [Serialize]
        private int rootObjectInstanceID;
#endif
        [Serialize]
        private Guid[] parentElementGuids;

        private GraphPointerData(GraphPointer pointer)
        {
#if UNITY_6000_4_OR_NEWER
            rootObjectInstanceID = pointer.rootObject.GetEntityId();
#else
            rootObjectInstanceID = pointer.rootObject.GetInstanceID();
#endif
            parentElementGuids = pointer.parentElementGuids.ToArray();
        }

        public static GraphPointerData FromPointer(GraphPointer pointer)
        {
            if (pointer == null || !pointer.isValid)
            {
                return null;
            }

            return new GraphPointerData(pointer);
        }

        public GraphReference ToReference(bool ensureValid)
        {
            UnityEngine.Object obj;

#if UNITY_6000_3_OR_NEWER
            obj = EditorUtility.EntityIdToObject(rootObjectInstanceID);
#else
            obj = EditorUtility.InstanceIDToObject(rootObjectInstanceID);
#endif
            return GraphReference.New(obj, parentElementGuids, ensureValid);
        }
    }
}
