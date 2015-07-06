

namespace XSockets.Protocol.Mqtt.Modules.Protocol
{
    using System.Text;
    using System.Collections.Generic;
    using Core.Common.Protocol;
    using Core.Common.Socket.Event.Arguments;
    using Core.Common.Socket.Event.Interface;

    public class MqttProtocolProxy : IProtocolProxy
    {
        public IMessage In(IEnumerable<byte> payload, MessageType messageType)
        {
            return null;
        }

        public byte[] Out(IMessage message)
        {
            if(!message.IsMqtt())
                return null;
            //Return part to publish...
            if (message.MessageType == MessageType.Binary)
                return message.ToBytes();
            else
            {
                return Encoding.UTF8.GetBytes(message.Data);
            }
        }
    }

    public static class MqttMessageHelpers
    {
        public static bool IsMqtt(this IMessage message)
        {
            switch (message.Topic)
            {
                case Core.Common.Globals.Constants.Events.Error:
                case Core.Common.Globals.Constants.Events.Controller.Closed:
                case Core.Common.Globals.Constants.Events.Controller.Opened:
                case Core.Common.Globals.Constants.Events.Controller.Init:
                case Core.Common.Globals.Constants.Events.PubSub.Subscribe:
                case Core.Common.Globals.Constants.Events.PubSub.Unsubscribe:
                case Core.Common.Globals.Constants.Events.Storage.Clear:
                case Core.Common.Globals.Constants.Events.Storage.Get:
                case Core.Common.Globals.Constants.Events.Storage.Remove:
                case Core.Common.Globals.Constants.Events.Storage.Set:
                case Core.Common.Globals.Constants.Messages.UnknownController:                
                    return false;
                default:
                    return true;
            }
        }
    }
}