# MQTT
A MQTT protocol plugin for XSockets based on [Paolo Patierno](https://twitter.com/ppatierno)s work with [GnatMQ](https://mqttbroker.codeplex.com/) / [M2MQTT](https://m2mqtt.wordpress.com/)

##Test MQTT in XSockets.NET

NOTE: Currently release on [nuget](https://www.nuget.org/packages/XSockets/5.0.0-beta4) as a pre-release

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
        
        //Add this class in the project if you want port 1883 instead
        //Note that you can add as many configuration classes as you like to get more endpoints.
        using XSockets.Core.Configuration;
        public class MqttConfig : ConfigurationSetting
        {
            public MqttConfig():base("ws://localhost:1883"){}
        }
        
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
  
 
## Anything To MQTT
**NOTE: This feature is not yet published to Nuget**  

Since you can connect anyhting that has TCP/IP to XSockets you can send a message to the MQTT clients from anything that has TCP/IP.

Below is a simple controller `Foo` that will accept any message to the method `Bar` and send that message to all subscribing MQTT clients.
    
    public class Foo : XSocketController
    {        
        public void Bar(IMessage message)
        {
            //Send to all clients MQTT excluded
            this.InvokeToAll(message);
            //Send to all MQTT clients, still just a POC, so it is kind of hacky
            foreach (var c in this.FindOn<MqttController>())
            {
                c.MqttClient.Publish(message.Topic,Encoding.UTF8.GetBytes(message.Data));
            }            
        }
    }
    
###To Test This...
First of all add the sample class `Foo` from above to your XSockets sample project.

1. Start XSockets server
2. Connect a MQTT client (MQTT FX)
3. Subscribe to `bar`
4. Open up Putty
5. Connect to the server (probably localhost and port 4502). Remember to choose `Raw` as connection type
6. Type in `PuttyProtocol` in putty and hit enter
7. Then type `foo|bar|Hello from putty` and hit enter

Your MQTT client should now get the message `Hello from putty` on the topic `bar` 
