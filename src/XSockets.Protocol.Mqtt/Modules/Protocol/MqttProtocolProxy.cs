namespace XSockets.Protocol.Mqtt.Modules.Protocol
{
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
            return null;
        }
    }
}