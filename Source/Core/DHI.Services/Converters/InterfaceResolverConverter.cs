namespace DHI.Services.Converters
{
    using System;

    /// <summary>
    ///     Create new converter for <typeparamref name="TInterface"/> mapped into <typeparamref name="TClass"/> 
    /// </summary>
    /// <typeparam name="TInterface">The interface of object need implement</typeparam>
    /// <typeparam name="TClass">The class of object that implement <typeparamref name="TInterface"/></typeparam>
    public class InterfaceResolverConverter<TInterface, TClass> : TypeResolverConverter<TInterface>
        where TClass : class, TInterface
    {
        public InterfaceResolverConverter(string typeDiscriminator = "$type") : base(typeDiscriminator)
        {
        }

        public override bool CanConvert(Type typeToConvert)
        {
            return typeof(TClass) == typeToConvert && base.CanConvert(typeToConvert);
        }
    }
}
