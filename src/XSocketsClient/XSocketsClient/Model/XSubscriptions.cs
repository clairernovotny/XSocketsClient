using System;
using System.Runtime.Serialization;

namespace XSocketsClient.Model
{
    /// <summary>
    /// Used for storing subscription for the clients
    /// </summary>
    [DataContract]
    public class XSubscriptions
    {
        /// <summary>
        /// The event to subscribe to
        /// </summary>
        [DataMember]
        public string Event { get; set; }
        /// <summary>
        /// The alias of the controller where the actionmethod is.
        /// This is set by the framework.
        /// </summary>
        [DataMember]
        public string Alias { get; set; }
        /// <summary>
        /// If true the framework will send a callback when the subscription is registered in the server
        /// </summary>
        [DataMember]
        public Boolean Confirm { get; set; }        
    }
}
