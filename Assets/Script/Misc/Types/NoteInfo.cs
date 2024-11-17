using MajdataPlay.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay.Types
{
    public unsafe class NoteInfo : ComponentInfo<IStateful<NoteStatus>>
    {
        public override bool IsFixedUpdatable => _fixedUpdate is not null;
        public override bool IsUpdatable => _update is not null;
        public override bool IsLateUpdatable => _lateUpdate is not null;
        public bool IsValid => _update is not null || 
                               _fixedUpdate is not null || 
                               _lateUpdate is not null;
        public NoteStatus State => Object?.State ?? NoteStatus.Destroyed;

       
        delegate void ComponentMethod();
        ComponentMethod? _update = null;
        ComponentMethod? _fixedUpdate = null;
        ComponentMethod? _lateUpdate = null;
        public NoteInfo(IStateful<NoteStatus> noteObj)
        {
            if (Object is IUpdatableComponent<NoteStatus> component)
                _update = new ComponentMethod(component.ComponentUpdate);
            if (Object is IFixedUpdatableComponent<NoteStatus> _component)
                _fixedUpdate = new ComponentMethod(_component.ComponentFixedUpdate);
            if (Object is ILateUpdatableComponent<NoteStatus> __component)
                _lateUpdate = new ComponentMethod(__component.ComponentLateUpdate);
        }
        public override void Update()
        {
            if(_update is not null)
            {
                if (CanExecute())
                    _update();
            }
        }
        public override void LateUpdate()
        {
            if (_lateUpdate is not null)
            {
                if (CanExecute())
                    _lateUpdate();
            }
        }
        public override void FixedUpdate()
        {
            if (_fixedUpdate is not null)
            {
                if (CanExecute())
                    _fixedUpdate();
            }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool CanExecute() => State is not (NoteStatus.Start or NoteStatus.Destroyed);
    }
}
