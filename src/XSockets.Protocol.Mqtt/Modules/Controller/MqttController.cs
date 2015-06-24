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
    /// This also contains state since that is built-in to XSockets controllers.
    /// </summary>
    [XSocketMetadata("mqtt")]
    public class MqttController : XSocketController
    {
        private MqttClient _mqttClient;

        [NoEvent]
        public void MqttOpen(MqttMsgConnect msg)
        {
            _mqttClient = new MqttClient(this.ProtocolInstance.Socket.Socket);
            ((MqttManager)Composable.GetExport<IXSocketController>(typeof(MqttManager))).RegisterClient(this._mqttClient,msg);            
        }

        [NoEvent]
        public void MqttClose(MqttMsgDisconnect msg)
        {            
            _mqttClient.OnMqttMsgDisconnected();            
        }

        [NoEvent]
        public void MqttUnsubscribe(MqttMsgUnsubscribe msg)
        {            
            _mqttClient.OnMqttMsgUnsubscribeReceived(msg.MessageId, msg.Topics);            
        }

        [NoEvent]
        public void MqttUnSubAck(MqttMsgUnsuback msg)
        {
            _mqttClient.OnMqttMsgUnsubscribed(msg.MessageId);
        }

        [NoEvent]
        public void MqttPubComp(MqttMsgPubcomp msg)
        {
            _mqttClient.OnMqttMsgPublished(msg.MessageId, true);
        }

        [NoEvent]
        public void MqttPubRel(MqttMsgPublish msg)
        {
            _mqttClient.OnMqttMsgPublishReceived(msg);
        }

        [NoEvent]
        public void MqttPubAck(MqttMsgPuback msg)
        {
            // raise published message event
            // (PUBACK received for QoS Level 1)
            _mqttClient.OnMqttMsgPublished(msg.MessageId, true);            
        }

        [NoEvent]
        public void MqttPublish(MqttMsgPublish msg)
        {
            //TODO: Fix this internalEvent stuff..
            //// PUBLISH message received in a published internal event, no publish succeeded
            //if (internalEvent.GetType() == typeof(MsgPublishedInternalEvent))
            //    mqttClient.OnMqttMsgPublished(msg.MessageId, false);
            //else
            // raise PUBLISH message received event 
            _mqttClient.OnMqttMsgPublishReceived(msg);                        
        }
        [NoEvent]
        public void MqttSubAck(MqttMsgSuback msg)
        {
            _mqttClient.OnMqttMsgSubscribed(msg);
        }
        [NoEvent]
        public void MqttSubscribe(MqttMsgSubscribe msg)
        {            
            // raise subscribe topic event (SUBSCRIBE message received)
            _mqttClient.OnMqttMsgSubscribeReceived(msg.MessageId, msg.Topics, msg.QoSLevels);            
        }

        [NoEvent]
        public void MqttPingReq(MqttMsgPingReq message)
        {            
            _mqttClient.Send(new MqttMsgPingResp());
        }
    }
}
