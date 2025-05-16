using MajdataPlay.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
#nullable enable
namespace MajdataPlay
{
    internal abstract class MajSingleton : MajComponent
    {
        delegate ref object RefGetter();

        readonly Type _getterType;
        readonly Type _singletionType;
        readonly Type _singletionManagerType;
        readonly PropertyInfo _instanceProp;
        readonly MethodInfo _freeMethod;
        readonly RefGetter _getter;
        
        protected MajSingleton()
        {
            _singletionType = GetType();
            _singletionManagerType = typeof(Majdata<>).MakeGenericType(_singletionType);
            _getterType = typeof(RefGetter);
            _instanceProp = _singletionManagerType.GetProperty("Instance");
            _getter = (RefGetter)_instanceProp.GetGetMethod().CreateDelegate(_getterType);
            _freeMethod = _singletionManagerType.GetMethod("Free");
        }
        protected override void Awake()
        {
            base.Awake();
            ref var instanceRef = ref _getter();
            if(instanceRef is not null)
            {
                throw new TypeInitializationException(_singletionType.FullName, new InvalidOperationException("A singleton of the current type already exists"));
            }
            else
            {
                instanceRef = Convert.ChangeType(this, _singletionType);
            }
            DontDestroyOnLoad(GameObject);
        }

        protected virtual void OnDestroy()
        {
            _freeMethod.Invoke(null, null);
        }
    }
}
