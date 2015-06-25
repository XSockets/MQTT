namespace XSockets.Protocol.Mqtt.Modules.Protocol
{
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

    /// <summary>
    /// This MQTT implementation is based on the work of 
    /// </summary>
    [Export(typeof(IXSocketProtocol), Rewritable = Rewritable.No)]
    public class MqttProtocol : XSocketProtocol, IMqttNetworkChannel
    {
        #region IMqttNetworkChannel
        public bool DataAvailable
        {
            get { return Socket.Socket.Available > 0; }
        }

        public int Receive(byte[] buffer)
        {
            return Socket.Socket.Receive(buffer);
        }

        public int Receive(byte[] buffer, int timeout)
        {
            // check data availability (timeout is in microseconds)
            if (Socket.Socket.Poll(timeout * 1000, SelectMode.SelectRead))
            {
                return Receive(buffer);
            }
            else
            {
                return 0;
            }
        }

        public int Send(byte[] buffer)
        {
            return Socket.Socket.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }

        public void Close()
        {
            Socket.Close();
        }

        public void Connect()
        {
            throw new NotImplementedException();
        }
        #endregion

        public override bool CanDoHeartbeat()
        {
            return false;
        }

        public override bool DoHandshake()
        {
            return true;
        }
        public override string HostResponse
        {
            get { throw new NotImplementedException(); }
        }

        public override bool Match(IList<byte> handshake)
        {
            if (!handshake.Any()) return false;
            return ((handshake[0] & MqttMsgBase.MSG_TYPE_MASK) >> MqttMsgBase.MSG_TYPE_OFFSET) == MqttMsgBase.MQTT_MSG_CONNECT_TYPE;
        }


        public MqttController Broker { get; set; }
        public override byte[] GetHostResponse()
        {
            try
            {
                var connectMessage = MqttMsgConnect.ParseHandshake(RawHandshake[0], (byte)MqttProtocolVersion.Version_3_1_1, this);                
                Broker.MqttOpen(connectMessage);
                Broker.Open();                          
                return new byte[0]; 
            }
            catch (Exception ex)
            {
                // 3.1.4.2 -  The Server MUST validate that the CONNECT Packet conforms to section 3.1
                // and close the Network Connection without sending a CONNACK if it does not conform
                Composable.GetExport<IXLogger>().Error(ex, "MQTT CONNECT FAILED");
                Disconnect(false);
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
            //Controllers.AddOrUpdate(Broker.Alias, Broker);
        }        
    }
}