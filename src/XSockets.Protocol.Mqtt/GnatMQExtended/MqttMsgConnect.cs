/*
Copyright (c) 2013, 2014 Paolo Patierno

All rights reserved. This program and the accompanying materials
are made available under the terms of the Eclipse Public License v1.0
and Eclipse Distribution License v1.0 which accompany this distribution. 

The Eclipse Public License is available at 
   http://www.eclipse.org/legal/epl-v10.html
and the Eclipse Distribution License is available at 
   http://www.eclipse.org/org/documents/edl-v10.php.

Contributors:
   Paolo Patierno - initial API and implementation and/or initial documentation
*/


namespace uPLibrary.Networking.M2Mqtt.Messages
{
    using System;
    using System.Linq;
    using System.Text;
    using Exceptions;
    using XSockets.Core.Common.Protocol;

    /// <summary>
    /// Class for CONNECT message from client to broker
    /// </summary>
    public partial class MqttMsgConnect : MqttMsgBase
    {
        public static MqttMsgConnect ParseHandshake(byte fixedHeaderFirstByte, byte protocolVersion, IXSocketProtocol protocol)
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
                digit = protocol.RawHandshake[i++];
                value += ((digit & 127) * multiplier);
                multiplier *= 128;
            } while ((digit & 128) != 0);

            // read bytes from socket...
            buffer = protocol.RawHandshake.Skip(i).Take(value).ToArray();

            // protocol name
            protNameUtf8Length = ((buffer[index++] << 8) & 0xFF00);
            protNameUtf8Length |= buffer[index++];
            protNameUtf8 = new byte[protNameUtf8Length];
            Array.Copy(buffer, index, protNameUtf8, 0, protNameUtf8Length);
            index += protNameUtf8Length;
            msg.protocolName = new String(Encoding.UTF8.GetChars(protNameUtf8));

            // [v3.1.1] wrong protocol name
            if (!msg.protocolName.Equals(PROTOCOL_NAME_V3_1) && !msg.protocolName.Equals(PROTOCOL_NAME_V3_1_1))
                throw new MqttClientException(MqttClientErrorCode.InvalidProtocolName);

            // protocol version
            msg.protocolVersion = buffer[index];
            index += PROTOCOL_VERSION_SIZE;

            // connect flags
            // [v3.1.1] check lsb (reserved) must be 0
            if ((msg.protocolVersion == PROTOCOL_VERSION_V3_1_1) &&
                ((buffer[index] & RESERVED_FLAG_MASK) != 0x00))
                throw new MqttClientException(MqttClientErrorCode.InvalidConnectFlags);

            isUsernameFlag = (buffer[index] & USERNAME_FLAG_MASK) != 0x00;
            isPasswordFlag = (buffer[index] & PASSWORD_FLAG_MASK) != 0x00;
            msg.willRetain = (buffer[index] & WILL_RETAIN_FLAG_MASK) != 0x00;
            msg.willQosLevel = (byte)((buffer[index] & WILL_QOS_FLAG_MASK) >> WILL_QOS_FLAG_OFFSET);
            msg.willFlag = (buffer[index] & WILL_FLAG_MASK) != 0x00;
            msg.cleanSession = (buffer[index] & CLEAN_SESSION_FLAG_MASK) != 0x00;
            index += CONNECT_FLAGS_SIZE;

            // keep alive timer
            msg.keepAlivePeriod = (ushort)((buffer[index++] << 8) & 0xFF00);
            msg.keepAlivePeriod |= buffer[index++];

            // client identifier [v3.1.1] it may be zero bytes long (empty string)
            clientIdUtf8Length = ((buffer[index++] << 8) & 0xFF00);
            clientIdUtf8Length |= buffer[index++];
            clientIdUtf8 = new byte[clientIdUtf8Length];
            Array.Copy(buffer, index, clientIdUtf8, 0, clientIdUtf8Length);
            index += clientIdUtf8Length;
            msg.clientId = new String(Encoding.UTF8.GetChars(clientIdUtf8));
            // [v3.1.1] if client identifier is zero bytes long, clean session must be true
            if ((msg.protocolVersion == PROTOCOL_VERSION_V3_1_1) && (clientIdUtf8Length == 0) && (!msg.cleanSession))
                throw new MqttClientException(MqttClientErrorCode.InvalidClientId);

            // will topic and will message
            if (msg.willFlag)
            {
                willTopicUtf8Length = ((buffer[index++] << 8) & 0xFF00);
                willTopicUtf8Length |= buffer[index++];
                willTopicUtf8 = new byte[willTopicUtf8Length];
                Array.Copy(buffer, index, willTopicUtf8, 0, willTopicUtf8Length);
                index += willTopicUtf8Length;
                msg.willTopic = new String(Encoding.UTF8.GetChars(willTopicUtf8));

                willMessageUtf8Length = ((buffer[index++] << 8) & 0xFF00);
                willMessageUtf8Length |= buffer[index++];
                willMessageUtf8 = new byte[willMessageUtf8Length];
                Array.Copy(buffer, index, willMessageUtf8, 0, willMessageUtf8Length);
                index += willMessageUtf8Length;
                msg.willMessage = new String(Encoding.UTF8.GetChars(willMessageUtf8));
            }

            // username
            if (isUsernameFlag)
            {
                usernameUtf8Length = ((buffer[index++] << 8) & 0xFF00);
                usernameUtf8Length |= buffer[index++];
                usernameUtf8 = new byte[usernameUtf8Length];
                Array.Copy(buffer, index, usernameUtf8, 0, usernameUtf8Length);
                index += usernameUtf8Length;
                msg.username = new String(Encoding.UTF8.GetChars(usernameUtf8));
            }

            // password
            if (isPasswordFlag)
            {
                passwordUtf8Length = ((buffer[index++] << 8) & 0xFF00);
                passwordUtf8Length |= buffer[index++];
                passwordUtf8 = new byte[passwordUtf8Length];
                Array.Copy(buffer, index, passwordUtf8, 0, passwordUtf8Length);
                index += passwordUtf8Length;
                msg.password = new String(Encoding.UTF8.GetChars(passwordUtf8));
            }

            return msg;
        }
    }
}