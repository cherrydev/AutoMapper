using System;
using System.Collections.Generic;

namespace AutoMapper
{
    /// <summary>
    /// Context information regarding resolution of a destination value
    /// </summary>
	public class ResolutionContext : IEquatable<ResolutionContext>
	{
        /// <summary>
        /// Mapping operation options
        /// </summary>
        public MappingOperationOptions Options { get; private set; }

        /// <summary>
        /// Current type map
        /// </summary>
	    public TypeMap TypeMap { get; private set; }
        
        /// <summary>
        /// Current property map
        /// </summary>
	    public PropertyMap PropertyMap { get; private set; }

        /// <summary>
        /// Current source type
        /// </summary>
	    public Type SourceType { get; private set; }

        /// <summary>
        /// Current attempted destination type
        /// </summary>
	    public Type DestinationType { get; private set; }

        /// <summary>
        /// Index of current collection mapping
        /// </summary>
	    public int? ArrayIndex { get; private set; }

        /// <summary>
        /// Source value
        /// </summary>
	    public object SourceValue { get; private set; }

        /// <summary>
        /// Destination value
        /// </summary>
	    public object DestinationValue { get; private set; }

        /// <summary>
        /// Parent resolution context
        /// </summary>
	    public ResolutionContext Parent { get; private set; }

        /// <summary>
        /// Instance cache for resolving circular references
        /// </summary>
	    public Dictionary<ResolutionContext, object> InstanceCache { get; private set; }

        /// <summary>
        /// Current mapping engine
        /// </summary>
        public IMappingEngine Engine { get; private set; }

        public ResolutionContext(TypeMap typeMap, object source, Type sourceType, Type destinationType, MappingOperationOptions options, IMappingEngine engine)
			: this(typeMap, source, null, sourceType, destinationType, options, engine)
	    {
	    }

        public ResolutionContext(TypeMap typeMap, object source, object destination, Type sourceType, Type destinationType, MappingOperationOptions options, IMappingEngine engine)
		{
			TypeMap = typeMap;
			SourceValue = source;
			DestinationValue = destination;
            AssignTypes(typeMap, sourceType, destinationType);
			InstanceCache = new Dictionary<ResolutionContext, object>();
            Options = options;
            Engine = engine;
        }

        public void SetResolvedDestinationValue(object destintationValue)
        {
            DestinationValue = destintationValue;
        }

        private void AssignTypes(TypeMap typeMap, Type sourceType, Type destinationType)
        {
            if (typeMap != null)
            {
                SourceType = typeMap.SourceType;
                DestinationType = typeMap.DestinationType;
            }
            else
            {
                SourceType = sourceType;
                DestinationType = destinationType;
            }
        }

		private ResolutionContext(ResolutionContext context, object sourceValue)
		{
			ArrayIndex = context.ArrayIndex;
			TypeMap = context.TypeMap;
			PropertyMap = context.PropertyMap;
			SourceType = context.SourceType;
			SourceValue = sourceValue;
			DestinationValue = context.DestinationValue;
			Parent = context;
			DestinationType = context.DestinationType;
			InstanceCache = context.InstanceCache;
            Options = context.Options;
		    Engine = context.Engine;
		}

		private ResolutionContext(ResolutionContext context, object sourceValue, Type sourceType)
		{
			ArrayIndex = context.ArrayIndex;
		    TypeMap = null;
			PropertyMap = context.PropertyMap;
			SourceType = sourceType;
			SourceValue = sourceValue;
			DestinationValue = context.DestinationValue;
			Parent = context;
			DestinationType = context.DestinationType;
			InstanceCache = context.InstanceCache;
            Options = context.Options;
            Engine = context.Engine;
        }

        private ResolutionContext(ResolutionContext context, TypeMap memberTypeMap, object sourceValue, object destinationValue, Type sourceType, Type destinationType)
        {
            TypeMap = memberTypeMap;
            SourceValue = sourceValue;
            DestinationValue = destinationValue;
            Parent = context;
            AssignTypes(memberTypeMap, sourceType, destinationType);
            InstanceCache = context.InstanceCache;
            Options = context.Options;
            Engine = context.Engine;
        }

	    private ResolutionContext(ResolutionContext context, object sourceValue, object destinationValue, TypeMap memberTypeMap, PropertyMap propertyMap)
		{
			TypeMap = memberTypeMap;
			PropertyMap = propertyMap;
			SourceValue = sourceValue;
            DestinationValue = destinationValue;
			Parent = context;
			InstanceCache = context.InstanceCache;
            SourceType = memberTypeMap.SourceType;
            DestinationType = memberTypeMap.DestinationType;
            Options = context.Options;
            Engine = context.Engine;
        }

		private ResolutionContext(ResolutionContext context, object sourceValue, object destinationValue, Type sourceType, PropertyMap propertyMap)
		{
			PropertyMap = propertyMap;
			SourceType = sourceType;
			SourceValue = sourceValue;
            DestinationValue = destinationValue;
			Parent = context;
			DestinationType = propertyMap.DestinationProperty.MemberType;
			InstanceCache = context.InstanceCache;
            Options = context.Options;
            Engine = context.Engine;
        }

		private ResolutionContext(ResolutionContext context, object sourceValue, TypeMap typeMap, Type sourceType, Type destinationType, int arrayIndex)
		{
			ArrayIndex = arrayIndex;
			TypeMap = typeMap;
			PropertyMap = context.PropertyMap;
			SourceValue = sourceValue;
			Parent = context;
			InstanceCache = context.InstanceCache;
            AssignTypes(typeMap, sourceType, destinationType);
            Options = context.Options;
            Engine = context.Engine;
        }

		public string MemberName
		{
			get
			{
				return PropertyMap == null
				       	? string.Empty
				       	: (ArrayIndex == null
				       	   	? PropertyMap.DestinationProperty.Name
				       	   	: PropertyMap.DestinationProperty.Name + ArrayIndex.Value);
			}
		}

		public bool IsSourceValueNull
		{
			get { return Equals(null, SourceValue); }
		}


		public ResolutionContext CreateValueContext(object sourceValue, Type sourceType)
		{
			return new ResolutionContext(this, sourceValue, sourceType);
		}

        public ResolutionContext CreateTypeContext(TypeMap memberTypeMap, object sourceValue, object destinationValue, Type sourceType, Type destinationType)
        {
            return new ResolutionContext(this, memberTypeMap, sourceValue, destinationValue, sourceType, destinationType);
        }

        public ResolutionContext CreatePropertyMapContext(PropertyMap propertyMap)
        {
            return new ResolutionContext(this, SourceValue, DestinationValue, SourceType, propertyMap);
        }

		public ResolutionContext CreateMemberContext(TypeMap memberTypeMap, object memberValue, object destinationValue, Type sourceMemberType, PropertyMap propertyMap)
		{
			return memberTypeMap != null
			       	? new ResolutionContext(this, memberValue, destinationValue, memberTypeMap, propertyMap)
			       	: new ResolutionContext(this, memberValue, destinationValue, sourceMemberType, propertyMap);
		}

		public ResolutionContext CreateElementContext(TypeMap elementTypeMap, object item, Type sourceElementType, Type destinationElementType, int arrayIndex)
		{
			return new ResolutionContext(this, item, elementTypeMap, sourceElementType, destinationElementType, arrayIndex);
		}

		public override string ToString()
		{
			return string.Format("Trying to map {0} to {1}.", SourceType.Name, DestinationType.Name);
		}

		public TypeMap GetContextTypeMap()
		{
			TypeMap typeMap = TypeMap;
			ResolutionContext parent = Parent;
			while ((typeMap == null) && (parent != null))
			{
				typeMap = parent.TypeMap;
				parent = parent.Parent;
			}
			return typeMap;
		}

		public PropertyMap GetContextPropertyMap()
		{
			PropertyMap propertyMap = PropertyMap;
			ResolutionContext parent = Parent;
			while ((propertyMap == null) && (parent != null))
			{
				propertyMap = parent.PropertyMap;
				parent = parent.Parent;
			}
			return propertyMap;
		}

		public bool Equals(ResolutionContext other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(other.TypeMap, TypeMap) && Equals(other.SourceType, SourceType) && Equals(other.DestinationType, DestinationType) && Equals(other.SourceValue, SourceValue);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (ResolutionContext)) return false;
			return Equals((ResolutionContext) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (TypeMap != null ? TypeMap.GetHashCode() : 0);
				result = (result*397) ^ (SourceType != null ? SourceType.GetHashCode() : 0);
				result = (result*397) ^ (DestinationType != null ? DestinationType.GetHashCode() : 0);
				result = (result*397) ^ (SourceValue != null ? SourceValue.GetHashCode() : 0);
				return result;
			}
		}

		public static ResolutionContext New<TSource>(TSource sourceValue)
		{
			return new ResolutionContext(null, sourceValue, typeof (TSource), null, new MappingOperationOptions(), Mapper.Engine);
		}
	}

}
