# MQTT
A MQTT protocol plugin for XSockets based on [Paolo Patierno](https://twitter.com/ppatierno)s work with [GnatMQ](https://mqttbroker.codeplex.com/) / [M2MQTT](https://m2mqtt.wordpress.com/)

##Test MQTT in XSockets.NET

NOTE: Currently release on [nuget](https://www.nuget.org/packages/XSockets/5.0.0-beta3) as a pre-release

Simplest way is to just create a .NET 4.5 Console Application in C#

1. Create a new Console Application in C# .NET 4.5
2. `Install-Package XSockets -pre`
3. Add start start up code for the XSockets server
        
        //using XSockets.Core.Common.Socket;
        //using XSockets.Plugin.Framework;
        using (var container = Composable.GetExport<IXSocketServerContainer>())
        {
            container.Start();
            Console.ReadLine();
        }
        
4. Download [MQTT FX](http://mqttfx.jfx4ee.org/index.php/download)
5. Open `MQTT FX` and set port to be `4502` (if you did not change this in XSockets to be something else)
6. Connect and test MQTT

## About The Implementation
The goal was to leave Paolo's implementation as intact as possible. So under source you can see a `GnatMQ` folder, that folder contains the code from GnatMQ at CodePlex. 

The only changes made was:
 - A few methods on the `MqttClient` was made Internal instead of `Private`
 - `MqttMsgConnect` class was made partial
 
The XSockets plugins created are found under the `Modules`folder:
 - `MqttController`is the XSockets controller that will receive Mqtt messages. This module has a MqttClient instance
 - `MqttManager` is a internal XSockets controller (singleton) that will be used instead of the default MqttBroker since XSockets has its own transport layer.
 - `MqttProtocol` the custom protocol that make sure that the MQTT clients will use the MQTT logic
  
 
