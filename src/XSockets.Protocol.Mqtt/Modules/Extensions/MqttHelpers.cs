using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using uPLibrary.Networking.M2Mqtt.Messages;
using XSockets.Core.Common.Socket;
using XSockets.Protocol.Mqtt.Modules.Controller;

namespace XSockets.Protocol.Mqtt.Modules.Extensions
{
    public static class MqttHelpers
    {
        #region Constants...

        // protocol name supported
        private const string PROTOCOL_NAME_V3_1 = "MQIsdp";
        private const string PROTOCOL_NAME_V3_1_1 = "MQTT"; // [v.3.1.1]

        // max length for client id (removed in 3.1.1)
        //private const int CLIENT_ID_MAX_LENGTH = 23;

        // variable header fields
        //private const byte PROTOCOL_NAME_LEN_SIZE = 2;
        //private const byte PROTOCOL_NAME_V3_1_SIZE = 6;
        //private const byte PROTOCOL_NAME_V3_1_1_SIZE = 4; // [v.3.1.1]
        private const byte PROTOCOL_VERSION_SIZE = 1;
        private const byte CONNECT_FLAGS_SIZE = 1;
        //private const byte KEEP_ALIVE_TIME_SIZE = 2;

        //private const byte PROTOCOL_VERSION_V3_1 = 0x03;
        private const byte PROTOCOL_VERSION_V3_1_1 = 0x04; // [v.3.1.1]
        //private const ushort KEEP_ALIVE_PERIOD_DEFAULT = 60; // seconds
        //private const ushort MAX_KEEP_ALIVE = 65535; // 16 bit

        // connect flags
        private const byte USERNAME_FLAG_MASK = 0x80;
        //private const byte USERNAME_FLAG_OFFSET = 0x07;
        //private const byte USERNAME_FLAG_SIZE = 0x01;
        private const byte PASSWORD_FLAG_MASK = 0x40;
        //private const byte PASSWORD_FLAG_OFFSET = 0x06;
        //private const byte PASSWORD_FLAG_SIZE = 0x01;
        private const byte WILL_RETAIN_FLAG_MASK = 0x20;
        //private const byte WILL_RETAIN_FLAG_OFFSET = 0x05;
        //private const byte WILL_RETAIN_FLAG_SIZE = 0x01;
        private const byte WILL_QOS_FLAG_MASK = 0x18;
        private const byte WILL_QOS_FLAG_OFFSET = 0x03;
        //private const byte WILL_QOS_FLAG_SIZE = 0x02;
        private const byte WILL_FLAG_MASK = 0x04;
        //private const byte WILL_FLAG_OFFSET = 0x02;
        //private const byte WILL_FLAG_SIZE = 0x01;
        private const byte CLEAN_SESSION_FLAG_MASK = 0x02;
        //private const byte CLEAN_SESSION_FLAG_OFFSET = 0x01;
        //private const byte CLEAN_SESSION_FLAG_SIZE = 0x01;
        // [v.3.1.1] lsb (reserved) must be now 0
        private const byte RESERVED_FLAG_MASK = 0x01;
        //private const byte RESERVED_FLAG_OFFSET = 0x00;
        //private const byte RESERVED_FLAG_SIZE = 0x01;

        #endregion
        public static void InvokeTo(this MqttController controller, IMessage message, byte qosLevel, bool retain)
        {
            //TODO
        }

        /// <summary>
        /// For being able to parse connect message from XSockets
        /// </summary>
        /// <param name="protocolVersion"></param>
        /// <param name="rawHandshake"></param>
        /// <returns></returns>
        public static MqttMsgConnect Parse(byte protocolVersion, byte[] rawHandshake)
        {
            byte[] buffer;
            int index = 0;
            int protNameUtf8Length;
            byte[] protNameUtf8;
            bool isUsernameFlag;
            bool isPasswordFlag;
            int clientIdUtf8Length;
            byte[] clientIdUtf8;
            int willTopicUtf8Length;
            byte[] willTopicUtf8;
            int willMessageUtf8Length;
            byte[] willMessageUtf8;
            int usernameUtf8Length;
            byte[] usernameUtf8;
            int passwordUtf8Length;
            byte[] passwordUtf8;
            MqttMsgConnect msg = new MqttMsgConnect();

            // get remaining length and allocate buffer
            var i = 1;
            int multiplier = 1;
            int value = 0;
            int digit = 0;
            do
            {
                digit = rawHandshake[i++];
                value += ((digit & 127) * multiplier);
                multiplier *= 128;
            } while ((digit & 128) != 0);

            // read bytes from socket...
            buffer = rawHandshake.Skip(i).Take(value).ToArray();

            // protocol name
            protNameUtf8Length = ((buffer[index++] << 8) & 0xFF00);
            protNameUtf8Length |= buffer[index++];
            protNameUtf8 = new byte[protNameUtf8Length];
            Array.Copy(buffer, index, protNameUtf8, 0, protNameUtf8Length);
            index += protNameUtf8Length;
            msg.ProtocolName = new String(Encoding.UTF8.GetChars(protNameUtf8));

            // [v3.1.1] wrong protocol name
            if (!msg.ProtocolName.Equals(PROTOCOL_NAME_V3_1) && !msg.ProtocolName.Equals(PROTOCOL_NAME_V3_1_1))
                throw new MqttClientException(MqttClientErrorCode.InvalidProtocolName);

            // protocol version
            msg.ProtocolVersion = buffer[index];
            index += PROTOCOL_VERSION_SIZE;

            // connect flags
            // [v3.1.1] check lsb (reserved) must be 0
            if ((msg.ProtocolVersion == PROTOCOL_VERSION_V3_1_1) &&
                ((buffer[index] & RESERVED_FLAG_MASK) != 0x00))
                throw new MqttClientException(MqttClientErrorCode.InvalidConnectFlags);

            isUsernameFlag = (buffer[index] & USERNAME_FLAG_MASK) != 0x00;
            isPasswordFlag = (buffer[index] & PASSWORD_FLAG_MASK) != 0x00;
            msg.WillRetain = (buffer[index] & WILL_RETAIN_FLAG_MASK) != 0x00;
            msg.WillQosLevel = (byte)((buffer[index] & WILL_QOS_FLAG_MASK) >> WILL_QOS_FLAG_OFFSET);
            msg.WillFlag = (buffer[index] & WILL_FLAG_MASK) != 0x00;
            msg.CleanSession = (buffer[index] & CLEAN_SESSION_FLAG_MASK) != 0x00;
            index += CONNECT_FLAGS_SIZE;

            // keep alive timer
            msg.KeepAlivePeriod = (ushort)((buffer[index++] << 8) & 0xFF00);
            msg.KeepAlivePeriod |= buffer[index++];

            // client identifier [v3.1.1] it may be zero bytes long (empty string)
            clientIdUtf8Length = ((buffer[index++] << 8) & 0xFF00);
            clientIdUtf8Length |= buffer[index++];
            clientIdUtf8 = new byte[clientIdUtf8Length];
            Array.Copy(buffer, index, clientIdUtf8, 0, clientIdUtf8Length);
            index += clientIdUtf8Length;
            msg.ClientId = new String(Encoding.UTF8.GetChars(clientIdUtf8));
            // [v3.1.1] if client identifier is zero bytes long, clean session must be true
            if ((msg.ProtocolVersion == PROTOCOL_VERSION_V3_1_1) && (clientIdUtf8Length == 0) && (!msg.CleanSession))
                throw new MqttClientException(MqttClientErrorCode.InvalidClientId);

            // will topic and will message
            if (msg.WillFlag)
            {
                willTopicUtf8Length = ((buffer[index++] << 8) & 0xFF00);
                willTopicUtf8Length |= buffer[index++];
                willTopicUtf8 = new byte[willTopicUtf8Length];
                Array.Copy(buffer, index, willTopicUtf8, 0, willTopicUtf8Length);
                index += willTopicUtf8Length;
                msg.WillTopic = new String(Encoding.UTF8.GetChars(willTopicUtf8));

                willMessageUtf8Length = ((buffer[index++] << 8) & 0xFF00);
                willMessageUtf8Length |= buffer[index++];
                willMessageUtf8 = new byte[willMessageUtf8Length];
                Array.Copy(buffer, index, willMessageUtf8, 0, willMessageUtf8Length);
                index += willMessageUtf8Length;
                msg.WillMessage = new String(Encoding.UTF8.GetChars(willMessageUtf8));
            }

            // username
            if (isUsernameFlag)
            {
                usernameUtf8Length = ((buffer[index++] << 8) & 0xFF00);
                usernameUtf8Length |= buffer[index++];
                usernameUtf8 = new byte[usernameUtf8Length];
                Array.Copy(buffer, index, usernameUtf8, 0, usernameUtf8Length);
                index += usernameUtf8Length;
                msg.Username = new String(Encoding.UTF8.GetChars(usernameUtf8));
            }

            // password
            if (isPasswordFlag)
            {
                passwordUtf8Length = ((buffer[index++] << 8) & 0xFF00);
                passwordUtf8Length |= buffer[index++];
                passwordUtf8 = new byte[passwordUtf8Length];
                Array.Copy(buffer, index, passwordUtf8, 0, passwordUtf8Length);
                index += passwordUtf8Length;
                msg.Password = new String(Encoding.UTF8.GetChars(passwordUtf8));
            }

            return msg;
        }
    }
}
