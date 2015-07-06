using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using XSockets.Core.Common.Socket;
using XSockets.Protocol.Mqtt.Modules.Controller;

namespace XSockets.Protocol.Mqtt.Modules.Extensions
{
    public static class MqttHelpers
    {
        public static void InvokeTo(this MqttController controller, IMessage message, byte qosLevel, bool retain)
        {
            //TODO
        }
    }
}
