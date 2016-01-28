
namespace XSockets.Protocol.Mqtt.Modules.Protocol
{

    using Extensions;
    using Core.Common.Socket.Event.Interface;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using uPLibrary.Networking.M2Mqtt;
    using uPLibrary.Networking.M2Mqtt.Messages;
    using Core.Common.Protocol;
    using Core.Common.Utility.Logging;
    using Plugin.Framework;
    using Plugin.Framework.Attributes;
    using Controller;
    using System.Threading.Tasks;
    using Core.XSocket.Helpers;
    using Core.Common.Socket;
    /// <summary>
    /// This MQTT implementation is based on the work of 
    /// </summary>
    [Export(typeof(IXSocketProtocol), Rewritable = Rewritable.No)]
    public class MqttProtocol : XSocketProtocol, IMqttNetworkChannel
    {
        #region IMqttNetworkChannel

        // mask, offset and size for fixed header fields
        internal const byte MSG_TYPE_MASK = 0xF0;
        internal const byte MSG_TYPE_OFFSET = 0x04;

        // MQTT message types
        internal const byte MQTT_MSG_CONNECT_TYPE = 0x01;

        public bool DataAvailable
        {
            get { return Transport.Socket.Available > 0; }
        }

        public int Receive(byte[] buffer)
        {
            return Transport.Socket.Receive(buffer);
        }

        public new int Receive(byte[] buffer, int timeout)
        {
            // check data availability (timeout is in microseconds)
            if (Transport.Socket.Poll(timeout * 1000, SelectMode.SelectRead))
            {
                return Receive(buffer);
            }
            else
            {
                return 0;
            }
        }

        public new int Send(byte[] buffer)
        {
            return Transport.Socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        public void Close()
        {
            Transport.Close();
        }

        public void Connect()
        {
            throw new NotImplementedException();
        }
        #endregion

        private readonly IXSocketAuthenticationPipeline _authenticationPipeline;

        public override bool CanDoHeartbeat()
        {
            return false;
        }

        public override async Task<bool> DoHandshake()
        {
            return await TaskAsyncHelper.True;
        }
        public override string HostResponse
        {
            get { throw new NotImplementedException(); }
        }

        public override async Task<bool> Match(IList<byte> handshake)
        {
            if (!handshake.Any()) return false;
            return ((handshake[0] & MSG_TYPE_MASK) >> MSG_TYPE_OFFSET) == MQTT_MSG_CONNECT_TYPE;
        }


        public MqttController Broker { get; set; }
        public override async Task<byte[]> GetHostResponse()
        {
            try
            {
                var connectMessage = MqttHelpers.Parse((byte) MqttProtocolVersion.Version_3_1_1, this.RawHandshake);
                //var connectMessage = MqttMsgConnect.Parse((byte)MqttProtocolVersion.Version_3_1_1, this.RawHandshake);
                this.ConnectionContext.Environment = new Dictionary<string, object>();  
                this.ConnectionContext.Environment.Add("mqttclient", connectMessage);
                ConnectionContext.User = _authenticationPipeline.GetPrincipal(this);

                Broker.MqttOpen(connectMessage);
                await Broker.Open();                          
                return new byte[0]; 
            }
            catch (Exception ex)
            {
                // 3.1.4.2 -  The Server MUST validate that the CONNECT Packet conforms to section 3.1
                // and close the Network Connection without sending a CONNACK if it does not conform
                Composable.GetExport<IXLogger>().Error(ex, "MQTT CONNECT FAILED");
                await Disconnect(false);
                return new byte[0];
            }
        }

        public override IXSocketProtocol NewInstance()
        {
            return new MqttProtocol();
        }

        public MqttProtocol()
        {
            Broker = new MqttController
            {
                ProtocolInstance = this,                
            };
            ProtocolProxy = new MqttProtocolProxy();
            _authenticationPipeline = Composable.GetExport<IXSocketAuthenticationPipeline>();
        }

        public override byte[] OnOutgoingFrame(IMessage message)
        {
            var frame = this.ProtocolProxy.Out(message);
            if (frame == null)
                return frame;

            return new MqttMsgPublish(message.Topic, frame).GetBytes((byte)MqttProtocolVersion.Version_3_1_1);
        }

        public void Accept()
        {
            //Dummy... will never be called since we do not use MqttTransport layer
        }
    }
}