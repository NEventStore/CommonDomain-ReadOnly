using System;
using System.Linq;

namespace CommonDomain.Persistence.EventStore
{
  public static class TypeExtensions
  {
    public static Boolean Implements(this Type type, Type interfaceType)
    {
      return  interfaceType != null &&
              type != null &&
             !type.IsAbstract &&
              type.IsClass &&
              type.GetInterfaces().Any(item => interfaceType.IsGenericTypeDefinition ? item.IsGenericType && item.GetGenericTypeDefinition() == interfaceType : item == interfaceType);
    }
  }
}
