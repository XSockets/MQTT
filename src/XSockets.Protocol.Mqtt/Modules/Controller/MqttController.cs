namespace XSockets.Protocol.Mqtt.Modules.Controller
{
    using uPLibrary.Networking.M2Mqtt;
    using uPLibrary.Networking.M2Mqtt.Messages;
    using Core.Common.Socket;
    using Core.Common.Socket.Event.Attributes;
    using Core.XSocket;
    using Plugin.Framework;
    using Plugin.Framework.Attributes;
    /// <summary>
    /// Custom XSockets controller as MQTT-Broker. 
    /// </summary>
    [XSocketMetadata("mqtt")]
    public class MqttController : XSocketController
    {
        public MqttClient MqttClient { get; set; }

        [NoEvent]
        public void MqttOpen(MqttMsgConnect msg)
        {
            MqttClient = new MqttClient(this.ProtocolInstance.Socket.Socket);
            ((MqttManager)Composable.GetExport<IXSocketController>(typeof(MqttManager))).RegisterClient(this.MqttClient,msg);            
        }
    }
}
