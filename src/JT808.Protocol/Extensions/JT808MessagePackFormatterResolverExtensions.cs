using JT808.Protocol.Formatters;
using JT808.Protocol.MessagePack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JT808.Protocol.Extensions
{
    /// <summary>
	///
	/// ref http://adamsitnik.com/Span/#span-must-not-be-a-generic-type-argument
	/// ref http://adamsitnik.com/Span/
	/// ref:MessagePack.Formatters.DynamicObjectTypeFallbackFormatter
	/// </summary>
	public static class JT808MessagePackFormatterResolverExtensions
    {
        private delegate void JT808SerializeMethod(object dynamicFormatter, ref JT808MessagePackWriter writer, object value, IJT808Config config);

        private delegate dynamic JT808DeserializeMethod(object dynamicFormatter, ref JT808MessagePackReader reader, IJT808Config config);

        private static readonly ConcurrentDictionary<Type, (object Value, JT808SerializeMethod SerializeMethod)> jT808Serializers = new ConcurrentDictionary<Type, (object, JT808SerializeMethod)>();

        private static readonly ConcurrentDictionary<Type, (object Value, JT808DeserializeMethod DeserializeMethod)> jT808Deserializes = new ConcurrentDictionary<Type, (object, JT808DeserializeMethod)>();

        /// <summary>
        /// JT808动态序列化
        /// </summary>
        /// <param name="objFormatter"></param>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        /// <param name="config"></param>
        public static void JT808DynamicSerialize(object objFormatter, ref JT808MessagePackWriter writer, object value, IJT808Config config)
        {
            Type type = value.GetType();
            TypeInfo ti = type.GetTypeInfo();
            if (!jT808Serializers.TryGetValue(type, out var formatterAndDelegate))
            {
                Type t = type;
                Type formatterType = typeof(IJT808MessagePackFormatter<>).MakeGenericType(t);
                ParameterExpression param0 = Expression.Parameter(typeof(object), "formatter");
                ParameterExpression param1 = Expression.Parameter(typeof(JT808MessagePackWriter).MakeByRefType(), "writer");
                ParameterExpression param2 = Expression.Parameter(typeof(object), "value");
                ParameterExpression param3 = Expression.Parameter(typeof(IJT808Config), "config");
                MethodInfo serializeMethodInfo = formatterType.GetRuntimeMethod("Serialize", new Type[3]
                {
                    typeof(JT808MessagePackWriter).MakeByRefType(),
                    t,
                    typeof(IJT808Config)
                });
                MethodCallExpression body = Expression.Call(Expression.Convert(param0, formatterType), serializeMethodInfo, param1, ti.IsValueType ? Expression.Unbox(param2, t) : Expression.Convert(param2, t), param3);
                JT808SerializeMethod lambda = Expression.Lambda<JT808SerializeMethod>(body, new ParameterExpression[4]
                {
                    param0,
                    param1,
                    param2,
                    param3
                }).Compile();
                formatterAndDelegate = (objFormatter, lambda);
                jT808Serializers.TryAdd(t, formatterAndDelegate);
            }
            formatterAndDelegate.Item2(formatterAndDelegate.Item1, ref writer, value, config);
        }

        /// <summary>
        /// JT808动态反序列化
        /// </summary>
        /// <param name="objFormatter"></param>
        /// <param name="reader"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        public static dynamic JT808DynamicDeserialize(object objFormatter, ref JT808MessagePackReader reader, IJT808Config config)
        {
            Type type = objFormatter.GetType();
            if (!jT808Deserializes.TryGetValue(type, out var formatterAndDelegate))
            {
                Type t = type;
                Type formatterType = typeof(IJT808MessagePackFormatter<>).MakeGenericType(t);
                ParameterExpression param0 = Expression.Parameter(typeof(object), "formatter");
                ParameterExpression param1 = Expression.Parameter(typeof(JT808MessagePackReader).MakeByRefType(), "reader");
                ParameterExpression param2 = Expression.Parameter(typeof(IJT808Config), "config");
                MethodInfo deserializeMethodInfo = type.GetRuntimeMethod("Deserialize", new Type[2]
                {
                    typeof(JT808MessagePackReader).MakeByRefType(),
                    typeof(IJT808Config)
                });
                MethodCallExpression body = Expression.Call(Expression.Convert(param0, type), deserializeMethodInfo, param1, param2);
                JT808DeserializeMethod lambda = Expression.Lambda<JT808DeserializeMethod>(body, new ParameterExpression[3]
                {
                    param0,
                    param1,
                    param2
                }).Compile();
                formatterAndDelegate = (objFormatter, lambda);
                jT808Deserializes.TryAdd(t, formatterAndDelegate);
            }
            return formatterAndDelegate.Item2(formatterAndDelegate.Item1, ref reader, config);
        }
    }
}
