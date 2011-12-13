using System;
using System.Collections.Generic;

namespace BTDB.IOC
{
    internal class ContanerRegistrationContext
    {
        readonly ContainerImpl _container;
        readonly Dictionary<Type, ICReg> _registrations;
        readonly List<object> _instances = new List<object>();

        internal ContanerRegistrationContext(ContainerImpl container, Dictionary<Type, ICReg> registrations)
        {
            _container = container;
            _registrations = registrations;
        }

        internal int SingletonCount { get; set; }

        internal void AddCReg(Type asType, ICReg registration)
        {
            _registrations.Add(asType, registration);
        }

        internal int AddInstance(object instance)
        {
            _instances.Add(instance);
            return _instances.Count - 1;
        }
    }
}