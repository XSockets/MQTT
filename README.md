# MQTT
A MQTT protocol plugin for XSockets based on [Paolo Patierno](https://twitter.com/ppatierno)s work with [GnatMQ](https://mqttbroker.codeplex.com/) / [M2MQTT](https://m2mqtt.wordpress.com/)

##Test MQTT in XSockets.NET

Currently release on [nuget](https://www.nuget.org/packages/XSockets/5.0.0-beta3) as a pre-release

Simplest way is to just create a .NET 4.5 Console Application in C#

1. Create a new Console Application in C# .NET 4.5
2. Install-Package XSockets -pre
3. Add start start up code for the XSockets server
        
        //using XSockets.Core.Common.Socket;
        using (var container = XSockets.Plugin.Framework.Composable.GetExport<IXSocketServerContainer>())
        {
            container.Start();
            Console.ReadLine();
        }
        
4. Download [MQTT FX](http://mqttfx.jfx4ee.org/index.php/download)
5. Open MQTT FX and set port to be 4502 (if you did not change this in XSockets to be something else)
6. Connect and test MQTT
