using System;
using XSocketsClient.Common.Event.Arguments;
using XSocketsClient.Common.Event.Interface;
using XSocketsClient.Common.Interfaces;

namespace XSocketsClient.Helpers
{    
    public static partial class XSocketHelper
    {
        /// <summary>
        /// If possible use the extension-method ToTextArgs for the controller instead
        /// </summary>
        /// <param name="client"></param>
        /// <param name="obj"></param>
        /// <param name="eventname"></param>
        /// <returns></returns>
        public static ITextArgs AsTextArgs(this IXSocketClient client, object obj, string eventname)
        {            
            return new TextArgs { @event = eventname.ToLower(), data = client.Serializer.SerializeToString(obj) };
        }

        /// <summary>
        /// Builds a ITextArgs object from a JSON string and a event name
        /// </summary>
        /// <param name="json"></param>
        /// <param name="eventname"></param>
        /// <returns></returns>
        public static ITextArgs AsTextArgsForJson(this string json, string eventname)
        {
            return new TextArgs { @event = eventname.ToLower(), data = json };
        }

        /// <summary>
        /// Use when sending binary data
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="eventname"></param>
        /// <returns></returns>
        public static IBinaryArgs AsBinaryArgs(this byte[] obj, string eventname)
        {
            return new BinaryArgs { @event = eventname.ToLower(), data = obj };
        }

        /// <summary>
        /// Deserialize JSON to a strongly typed object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T Deserialize<T>(this IXSocketClient client, string json)
        {
            return client.Serializer.DeserializeFromString<T>(json);
        }

        /// <summary>
        /// If possible use the extension-method ToTextArgs for the controller instead
        /// </summary>
        /// <param name="client"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize(this IXSocketClient client, object obj)
        {
            return client.Serializer.SerializeToString(obj);
        }

        /// <summary>
        /// Deserialize JSON to a strongly typed object.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="targetType"></param>
        /// <param name="json"></param>
        /// <returns></returns>
        internal static object GetObject(this IXSocketClient client, Type targetType, string json)
        {
            return client.Serializer.DeserializeFromString(json, targetType);
        }
    }
}