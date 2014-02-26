using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using XSocketsClient.Common.Event.Interface;

namespace XSocketsClient.Common.Event.Arguments
{
    [DataContract]
    public class BinaryArgs : EventArgs, IBinaryArgs
    {
        public BinaryArgs()
        {
        }

        public BinaryArgs(IList<byte> data, string controller = null)
        {
            this.data = data;
            this.controller = controller;
        }

        [DataMember(Name = "event", IsRequired = true)]
        public string @event { get;  set; }
        [DataMember(Name = "data", IsRequired = true)]
        public IList<byte> data { get; set; }
        [DataMember(Name = "controller", IsRequired = true)]
        public string controller { get; set; }
    }
}