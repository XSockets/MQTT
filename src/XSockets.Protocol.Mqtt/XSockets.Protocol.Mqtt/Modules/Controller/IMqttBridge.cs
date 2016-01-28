

namespace XSockets.Protocol.Mqtt.Modules.Controller
{
    using uPLibrary.Networking.M2Mqtt;
    using uPLibrary.Networking.M2Mqtt.Messages;
    using XSockets.Plugin.Framework.Attributes;

    [Export(typeof(IMqttBridge))]
    public interface IMqttBridge
    {
        /// <summary>
        /// Will fire when a new publish message is received from a MQTT client.
        /// If you return false this message will not be published to MQTT clients.
        /// 
        /// Use this method to send messages to non-MQTT clients
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool OnPublish(MqttClient client, MqttMsgPublishEventArgs message);

        /// <summary>
        /// Will fire when a new subscription request is received.
        /// Remove topics not allowed from the MqttMsgSubscribeEventArgs to deny subscriptions.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        void OnSubscribe(MqttClient client, MqttMsgSubscribeEventArgs message);

        /// <summary>
        /// Will fire when a MQTT client unsubscribes
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        void OnUnsubscribe(MqttClient client, MqttMsgUnsubscribeEventArgs message);

        /// <summary>
        /// Will fire when a MQTT client disconnects
        /// </summary>
        /// <param name="client"></param>
        void OnClientDisconnect(MqttClient client);

        /// <summary>
        /// Will fire when a MQTT client connects
        /// </summary>
        /// <param name="client"></param>
        void OnClientConnect(MqttClient client, MqttMsgConnectEventArgs message);

        /// <summary>
        /// For publishing a message from XSockets to MQTT clients.
        /// 
        /// Normally you should use the extensions for sending binary or text data,
        /// but if you built a MqttMsgPublish object you may call this directly
        /// </summary>
        /// <param name="message"></param>
        void PublishToMqttClients(MqttMsgPublish message);
    }    
}
