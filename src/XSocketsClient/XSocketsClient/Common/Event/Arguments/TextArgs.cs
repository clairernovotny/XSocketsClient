using System;
using System.Runtime.Serialization;
using XSocketsClient.Common.Event.Interface;

namespace XSocketsClient.Common.Event.Arguments
{
    [DataContract]
    public class TextArgs : EventArgs, ITextArgs
    {
        public TextArgs()
        {
        }

        public TextArgs(string data, string @event, string controller = null)
        {
            this.data = data;
            this.@event = @event;
            this.controller = controller;
        }

        [DataMember(Name = "event", IsRequired = true)]
        public string @event { get;  set; }
        [DataMember(Name = "data", IsRequired = true)]
        public string data { get;  set; }
        [DataMember(Name = "controller", IsRequired = false)]
        public string controller { get; set; }        
    }
}