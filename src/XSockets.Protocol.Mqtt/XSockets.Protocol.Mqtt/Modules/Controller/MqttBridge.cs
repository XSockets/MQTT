
namespace XSockets.Protocol.Mqtt.Modules.Controller
{
    using Core.Common.QoS;
    using Core.Common.Socket;
    using Core.Common.Utility.Logging;
    using Plugin.Framework;
    using uPLibrary.Networking.M2Mqtt;
    using uPLibrary.Networking.M2Mqtt.Messages;
    using XSockets.Plugin.Framework.Attributes;

    [Export(typeof(IMqttBridge), Rewritable = Rewritable.Yes)]
    public class MqttBridge : IMqttBridge
    {
        public virtual void OnClientConnect(MqttClient client, MqttMsgConnectEventArgs message)
        {
            Composable.GetExport<IXLogger>().Verbose("Mqtt Client connect {@c}, {@m}", client, message);
            return;
        }

        public virtual void OnClientDisconnect(MqttClient client)
        {
            Composable.GetExport<IXLogger>().Verbose("Mqtt Client Disconnect {@m}", client);
            return;
        }

        public virtual bool OnPublish(MqttClient client, MqttMsgPublishEventArgs message)
        {            
            Composable.GetExport<IXLogger>().Verbose("Mqtt Publish {@m}", message);            
            return true;
        }

        public virtual void OnSubscribe(MqttClient client, MqttMsgSubscribeEventArgs message)
        {
            Composable.GetExport<IXLogger>().Verbose("Mqtt Subscribe {@m}", message);            
            return;
        }

        public virtual void OnUnsubscribe(MqttClient client, MqttMsgUnsubscribeEventArgs message)
        {
            Composable.GetExport<IXLogger>().Verbose("Mqtt Unubscribe {@m}", message);
            return;
        }

        public virtual void PublishToMqttClients(MqttMsgPublish message)
        {
            ((MqttManager)Composable.GetExport<IXSocketController>(typeof(MqttManager))).publisherManager.Publish(message);            
        }
    }
}
