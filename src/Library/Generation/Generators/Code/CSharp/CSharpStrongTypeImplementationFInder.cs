using System;
using System.Linq;
using Atom.Data;
using Atom.Data.Types;
using Atom.Generation.Extensions;
using Atom.Types;

namespace Atom.Generation.Generators.Code.CSharp
{
    public class CSharpStrongTypeImplementationFinder : StrictTypeVisitor<string>
    {
        private readonly AtomMemberInfo _member;

        public CSharpStrongTypeImplementationFinder(AtomMemberInfo member)
        {
            _member = member;
        }

        public override string Visit(MemberGuid memberGuid)
        {
            string typeName = StringExt.ToTitleCase(_member.Name);

            var template = $@"
[Serializable]
[JsonConverter(typeof(GuidStrongTypeConverter<{typeName}>))]
public partial struct {typeName} : ITypedGuid, IStrongTypeFactory<Guid, {typeName}>, IEquatable<{typeName}>
{{
    private readonly Guid _value;

    private {typeName}(Guid guid)
    {{
        _value = guid;
    }}

    public static explicit operator {typeName}(Guid value)
    {{
        return new {typeName}(value);
    }}

    public static implicit operator Guid({typeName} value)
    {{
        return value._value;
    }}

    public override string ToString()
    {{
        return _value.ToString();
    }}

    private Guid UnderlyingValue()
    {{
        return _value;
    }}

    public Guid Value {{ get {{ return _value; }} }}

    {typeName} IStrongTypeFactory<Guid, {typeName}>.NewWithValue(Guid value)
    {{
        return new {typeName}(value);
    }}

    public TResult Accept<TResult>(IStrongTypeVistor<TResult> visitor)
    {{
        return visitor.Visit(this);
    }}

    public Guid ToGuid()
    {{
        return _value;
    }}

    public string ToString(string format)
    {{
        return _value.ToString(format);
    }}

    public string ToString(string format, IFormatProvider provider)
    {{
        return _value.ToString(format, provider);
    }}

    public override bool Equals(object other)
    {{
        if (other is {typeName})
        {{
            return Equals(({typeName})other);
        }}
        return false;
    }}

    public override int GetHashCode()
    {{
        return _value.GetHashCode();
    }}

    public bool Equals({typeName} other)
    {{
        return _value.Equals(other._value);
    }}

    public static bool operator ==({typeName} left, {typeName} right)
    {{
        return left.Equals(right);
    }}

    public static bool operator !=({typeName} left, {typeName} right)
    {{
        return !left.Equals(right);
    }}
}}";
            return template;
        }

        public override string Visit(MemberText memberText)
        {
            string typeName = StringExt.ToTitleCase(_member.Name);

            var template = $@"
[Serializable]
[JsonConverter(typeof(StringStrongTypeConverter<{typeName}>))]
public partial struct {typeName} : ITypedString,  IStrongTypeFactory<string, {typeName}>, IEquatable<{typeName}>
{{
    private readonly string _value;

    private {typeName}(string value)
    {{
        _value = value;
    }}

    public static explicit operator {typeName}(string value)
    {{
        return new {typeName}(value);
    }}

    public static implicit operator string({typeName} value)
    {{
        return value._value;
    }}

    public override string ToString()
    {{
        return _value;
    }}

    public string UnderlyingValue()
    {{
        return _value;
    }}

    public string Value {{ get {{ return _value; }} }}

    {typeName} IStrongTypeFactory<string, {typeName}>.NewWithValue(string value)
    {{
        return new {typeName}(value);
    }}

    public TResult Accept<TResult>(IStrongTypeVistor<TResult> visitor)
    {{
        return visitor.Visit(this);
    }}

    public override bool Equals(object other)
    {{
        if (other is {typeName})
        {{
            return Equals(({typeName})other);
        }}
        return false;
    }}

    public override int GetHashCode()
    {{
        return _value.GetHashCode();
    }}

    public bool Equals({typeName} other)
    {{
        if(_value == null)
        {{
            if(other._value == null)
            {{
                return true;
            }}

            return false;
        }}

        return _value.Equals(other._value);
    }}

    public static bool operator ==({typeName} left, {typeName} right)
    {{
        return left.Equals(right);
    }}

    public static bool operator !=({typeName} left, {typeName} right)
    {{
        return !left.Equals(right);
    }}
}}";
            return template;
        }

        public override string Visit(MemberFloat memberText)
        {
            string typeName = StringExt.ToTitleCase(_member.Name);

            return $@"[Serializable]
[JsonConverter(typeof(FloatStrongTypeConverter<{typeName}>))]
public partial struct {typeName} : ITypedFloat, IStrongTypeFactory<float, {typeName}>, IEquatable<{typeName}>
{{
    private readonly float _value;

    private {typeName}(float @float)
    {{
        _value = @float;
    }}

    public static explicit operator {typeName}(float value)
    {{
        return new {typeName}(value);
    }}

    public static implicit operator float({typeName} value)
    {{
        return value._value;
    }}

    public float UnderlyingValue()
    {{
        return _value;
    }}

    public float Value {{ get {{ return _value; }} }}

    {typeName} IStrongTypeFactory<float, {typeName}>.NewWithValue(float value)
    {{
        return new {typeName}(value);
    }}

    public TResult Accept<TResult>(IStrongTypeVistor<TResult> visitor)
    {{
        return visitor.Visit(this);
    }}

    public override string ToString()
    {{
        return _value.ToString();
    }}

    public string ToString(string format)
    {{
        return _value.ToString(format);
    }}

    public string ToString(IFormatProvider provider)
    {{
        return _value.ToString(provider);
    }}

    public string ToString(string format, IFormatProvider provider)
    {{
        return _value.ToString(format, provider);
    }}

    public override bool Equals(object other)
    {{
        if (other is {typeName})
        {{
            return Equals(({typeName})other);
        }}
        return false;
    }}

    public override int GetHashCode()
    {{
        return _value.GetHashCode();
    }}

    public bool Equals({typeName} other)
    {{
        return _value.Equals(other._value);
    }}

    public static bool operator ==({typeName} left, {typeName} right)
    {{
        return left.Equals(right);
    }}

    public static bool operator !=({typeName} left, {typeName} right)
    {{
        return !left.Equals(right);
    }}
}}";
        }

        public override string Visit(MemberBinary memberText)
        {
            return null;
        }

        public override string Visit(MemberLong memberLong)
        {
            return GetPrimitiveNumericStrongType("long");
        }


        private string GetPrimitiveNumericStrongType(string primitiveTypeName)
        {
            if (_member.HasReference && _member.Reference.TargetMember.Atom.IsLookup)
            {
                return EnumDefinition(ProjectedAtomRoot.FromAtom(_member.Reference.TargetMember.Atom));
            }

            string TypeName = StringExt.ToTitleCase(_member.Name);
            string lookupTemplateExtension = string.Empty;

            if (_member.Atom.IsLookup)
            {
                string
                    LookupName = StringExt.ToTitleCase(_member.Atom.Name);

                lookupTemplateExtension = $@"
    public static implicit operator {LookupName}({TypeName} id)
    {{
        return ({LookupName}) ({primitiveTypeName}) (id);
    }}

    public static implicit operator {TypeName}({LookupName} value)
    {{
        return ({TypeName})({primitiveTypeName})(value);
    }}";

            }

            var upperCasedPrimitiveTypeName = char.ToUpper(primitiveTypeName[0]) + primitiveTypeName.Substring(1);
            var template = $@"
[Serializable]
[JsonConverter(typeof({upperCasedPrimitiveTypeName}StrongTypeConverter<{TypeName}>))]
public partial struct {TypeName} : ITyped{upperCasedPrimitiveTypeName}, IStrongTypeFactory<{primitiveTypeName}, {TypeName}>, IEquatable<{TypeName}>
{{
    private readonly {primitiveTypeName} _{primitiveTypeName};

    private {TypeName}({primitiveTypeName} @{primitiveTypeName})
    {{
        _{primitiveTypeName} = @{primitiveTypeName};
    }}

    public static explicit operator {TypeName}({primitiveTypeName} value)
    {{
        return new {TypeName}(value);
    }}

    public static implicit operator {primitiveTypeName}({TypeName} value)
    {{
        return value._{primitiveTypeName};
    }}

    public {primitiveTypeName} UnderlyingValue()
    {{
        return _{primitiveTypeName};
    }}

    public {primitiveTypeName} Value {{ get {{ return _{primitiveTypeName}; }} }}

    {TypeName} IStrongTypeFactory<{primitiveTypeName}, {TypeName}>.NewWithValue({primitiveTypeName} value)
    {{
        return new {TypeName}(value);
    }}

    public TResult Accept<TResult>(IStrongTypeVistor<TResult> visitor)
    {{
        return visitor.Visit(this);
    }}
    public override string ToString()
    {{
        return _{primitiveTypeName}.ToString();
    }}

    public string ToString(string format)
    {{
        return _{primitiveTypeName}.ToString(format);
    }}

    public string ToString(string format, IFormatProvider provider)
    {{
        return _{primitiveTypeName}.ToString(format, provider);
    }}

    public string ToString(IFormatProvider provider)
    {{
        return _{primitiveTypeName}.ToString(provider);
    }}

    public override bool Equals(object other)
    {{
        if (other is {TypeName})
        {{
            return Equals(({TypeName})other);
        }}
        return false;
    }}

    public override int GetHashCode()
    {{
        return _{primitiveTypeName}.GetHashCode();
    }}

    public bool Equals({TypeName} other)
    {{
        return _{primitiveTypeName}.Equals(other._{primitiveTypeName});
    }}

    public static bool operator ==({TypeName} left, {TypeName} right)
    {{
        return left.Equals(right);
    }}

    public static bool operator !=({TypeName} left, {TypeName} right)
    {{
        return !left.Equals(right);
    }}

    {lookupTemplateExtension}
}}";

            return template;
        }

        private string EnumDefinition(ProjectedAtomRoot arg)
        {
            var members = arg.BasedOn.Lookup.Values.Select((value, idx) => GetEnumDefinitionType(value, idx, arg.BasedOn.Lookup.NamePrefix));

            string enumMembers = string.Join("," + Environment.NewLine, members.IndentAllLines(1));
            var type = new CSharpDefaultTypeFinder(optional: false).Visit(arg.BasedOn.Members.First().MemberType);

            var template = $@"
[Serializable, DataContract]
public enum {arg.Name} : {type} {{
    {enumMembers}
}}
";

            return template;
        }

        private string GetEnumDefinitionType(LookupValue element, int idx, string namePrefix)
        {
            return $@"
/// <summary>
/// {element.Description}
/// </summary>
[EnumMember]
{namePrefix}{element.Name} = {element.Index ?? (idx + 1)}";
        }

        public override string Visit(MemberDateTime memberDateTime)
        {
            return DateTimeType(memberDateTime);
        }

        private string DateTimeType(MemberType memberDateTime)
        {
            string typeName = StringExt.ToTitleCase(_member.Name);
            return
                $@"[Serializable]
[JsonConverter(typeof(DateTimeStrongTypeConverter<{typeName}>))]
public partial struct {typeName} : ITypedDateTime,  IStrongTypeFactory<DateTime, {typeName}>, IEquatable<{typeName}>
{{
    private readonly DateTime _value;

    private {typeName}(DateTime value)
    {{
        _value = value;
    }}

    public static explicit operator {typeName}(DateTime value)
    {{
        return new {typeName}(value);
    }}

    public static implicit operator DateTime({typeName} value)
    {{
        return value._value;
    }}

    public override string ToString()
    {{
        return _value.ToString();
    }}

    public string ToString(string format)
    {{
        return _value.ToString(format);
    }}

    public string ToString(IFormatProvider provider)
    {{
        return _value.ToString(provider);
    }}

    public string ToString(string format, IFormatProvider provider)
    {{
        return _value.ToString(format, provider);
    }}

    public DateTime UnderlyingValue()
    {{
        return _value;
    }}

    public DateTime Value {{ get {{ return _value; }} }}

    {typeName} IStrongTypeFactory<DateTime, {typeName}>.NewWithValue(DateTime value)
    {{
        return new {typeName}(value);
    }}

    public TResult Accept<TResult>(IStrongTypeVistor<TResult> visitor)
    {{
        return visitor.Visit(this);
    }}

    public override bool Equals(object other)
    {{
        if (other is {typeName})
        {{
            return Equals(({typeName})other);
        }}
        return false;
    }}

    public override int GetHashCode()
    {{
        return _value.GetHashCode();
    }}

    public bool Equals({typeName} other)
    {{
        return _value.Equals(other._value);
    }}

    public static bool operator ==({typeName} left, {typeName} right)
    {{
        return left.Equals(right);
    }}

    public static bool operator !=({typeName} left, {typeName} right)
    {{
        return !left.Equals(right);
    }}
}}";
        }

        public override string Visit(MemberDate memberDate)
        {
            return DateTimeType(memberDate);
        }

        public override string Visit(MemberBool memberBool)
        {
            return null;
        }

        public override string Visit(MemberDecimal memberDecimal)
        {
            return null;
        }

        public override string Visit(MemberByte memberByte)
        {
            return GetPrimitiveNumericStrongType("byte");
        }

        public override string Visit(MemberShort memberShort)
        {
            return GetPrimitiveNumericStrongType("short");
        }

        public override string Visit(MemberDouble memberDouble)
        {
            return null;
        }
    }
}
