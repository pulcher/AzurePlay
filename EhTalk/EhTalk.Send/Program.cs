﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.ServiceBus.Messaging;

namespace EhTalk.Send
{
    class Program
    {
        static string eventHubName = "nddtest";
        static string connectionString = "Endpoint=sb://trustme.servicebus.windows.net/;SharedAccessKeyName=writer;SharedAccessKey=I/1xJXFJIQ3cITHvhRrvskGtRaGzB0F6/lGMgEWjE1g=";

        static void Main(string[] args)
        {
            Console.WriteLine("Press Ctrl-C to stop the sender process");
            Console.WriteLine("Press Enter to start now");
            Console.ReadLine();
            SendingRandomMessages();
        }

        static void SendingRandomMessages()
        {
            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, eventHubName);
            while (true)
            {
                try
                {
                    var message = Guid.NewGuid().ToString();
                    Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, message);
                    eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(message)));
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                    Console.ResetColor();
                    Console.ReadLine();
                }

                Thread.Sleep(200);
            }
        }
    }
}
