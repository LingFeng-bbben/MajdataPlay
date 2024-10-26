using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Reflection;

namespace MychIO.Connection
{

    // Note all public properties of this class will be added to the _properties object
    // Properties should only be updated on object construction through the constructor
    public abstract class ConnectionProperties : IConnectionProperties
    {
        private IDictionary<string, dynamic> _properties;

        public string Id { get; private set; }

        public IConnectionProperties UpdateProperties(IDictionary<string, dynamic> updateProperties)
        {
            _properties = MergeProperties(_properties, updateProperties);
            UpdateFieldsFromProperties();
            Id = _properties.TryGetValue("Id", out var id) && id is string v ? v : Guid.NewGuid().ToString();
            return this;
        }

        protected static IDictionary<string, dynamic> MergeProperties(
            IDictionary<string, dynamic> overWrittenProperties, IDictionary<string, dynamic> updateProperties)
        {
            var result = new Dictionary<string, dynamic>(overWrittenProperties);
            foreach (var (key, value) in updateProperties)
            {
                result[key] = value;
            }
            return result;
        }

        protected void PopulatePropertiesFromFields()
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var value = field.GetValue(this);
                if (null == value)
                {
                    continue;
                }
                _properties[field.Name] = value;
            }
        }

        protected void UpdateFieldsFromProperties()
        {
            var fields = GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (!_properties.TryGetValue(field.Name, out var value))
                {
                    continue;
                }
                try
                {
                    field.SetValue(this, value);
                }
                catch (Exception) { }
            }
        }

        public IDictionary<string, dynamic> GetProperties() => _properties;

        public abstract ConnectionType GetConnectionType();

    }
}