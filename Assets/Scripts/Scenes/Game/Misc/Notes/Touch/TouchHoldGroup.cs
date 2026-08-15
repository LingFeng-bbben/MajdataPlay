using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace MajdataPlay.Scenes.Game.Notes.Touch;
public sealed class TouchHoldGroup
{
    public float Percent
    {
        get
        {
            if (_memberCount == 0)
            {
                return 0f;
            }
            return _triggeredCount / (float)_memberCount;
        }
    }
    public object[] Members 
    { 
        get
        {
            return _members;
        }
        set
        {
            _members = value ?? Array.Empty<IStatefulNote>();
            _triggeredMembers = new(_members.Length);
            _triggeredCount = 0;
            _memberCount = _members.Length;
        }
    }

    object[] _members = Array.Empty<IStatefulNote>();
    HashSet<EntityId> _triggeredMembers = new();
    int _triggeredCount = 0;
    int _memberCount = 0;

    public void RegisterTrigger(EntityId instanceID)
    {
        if(_triggeredMembers.Add(instanceID))
        {
            _triggeredCount++;
        }
    }
    public void UnregisterTrigger(EntityId instanceID)
    {
        if(_triggeredMembers.Remove(instanceID))
        {
            _triggeredCount--;
        }     
    }
    public void Exit()
    {
        _memberCount--;
    }
}
