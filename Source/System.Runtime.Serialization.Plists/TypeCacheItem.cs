

namespace System.Runtime.Serialization.Plists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal sealed class TypeCacheItem
    {
        private Type type;
        private bool hasCustomContract;

        public TypeCacheItem(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type", "type cannot be null.");
            }

            this.type = type;
            this.hasCustomContract = type.GetCustomAttributes(typeof(DataContractAttribute), false).Length > 0;
            this.InitializeFields();
            this.InitializeProperties();
        }

        public IList<DataMemberAttribute> FieldMembers { get; private set; }
        
        public IList<FieldInfo> Fields { get; private set; }

        public IList<PropertyInfo> Properties { get; private set; }

        public IList<DataMemberAttribute> PropertyMembers { get; private set; }

        private void InitializeFields()
        {
            this.FieldMembers = new List<DataMemberAttribute>();
            this.Fields = new List<FieldInfo>();

            var fields = this.hasCustomContract ?
                this.type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) :
                this.type.GetFields(BindingFlags.Instance | BindingFlags.Public);

            var tuples = from f in fields
                         let attr = f.GetCustomAttributes(false)
                         let member = attr.OfType<DataMemberAttribute>().FirstOrDefault()
                         where !f.IsLiteral && attr.OfType<IgnoreDataMemberAttribute>().Count() == 0
                         select new
                         {
                             Info = f,
                             Member = member
                         };

            foreach (var tuple in tuples.Where(t => !this.hasCustomContract || t.Member != null))
            {
                DataMemberAttribute member = tuple.Member != null ?
                    tuple.Member :
                    new DataMemberAttribute()
                    {
                        EmitDefaultValue = true,
                        IsRequired = false
                    };

                member.Name = !String.IsNullOrEmpty(member.Name) ? member.Name : tuple.Info.Name;

                this.FieldMembers.Add(member);
                this.Fields.Add(tuple.Info);
            }
        }

        private void InitializeProperties()
        {
            this.Properties = new List<PropertyInfo>();
            this.PropertyMembers = new List<DataMemberAttribute>();

            var properties = this.hasCustomContract ?
                this.type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) :
                this.type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var tuples = from p in properties
                         let attr = p.GetCustomAttributes(false)
                         let member = attr.OfType<DataMemberAttribute>().FirstOrDefault()
                         where p.CanRead && p.CanWrite && attr.OfType<IgnoreDataMemberAttribute>().Count() == 0
                         select new
                         {
                             Info = p,
                             Member = member
                         };

            foreach (var tuple in tuples.Where(t => !this.hasCustomContract || t.Member != null))
            {
                DataMemberAttribute member = tuple.Member != null ?
                    tuple.Member :
                    new DataMemberAttribute()
                    {
                        EmitDefaultValue = true,
                        IsRequired = false
                    };

                member.Name = !String.IsNullOrEmpty(member.Name) ? member.Name : tuple.Info.Name;

                this.PropertyMembers.Add(member);
                this.Properties.Add(tuple.Info);
            }
        }
    }
}
