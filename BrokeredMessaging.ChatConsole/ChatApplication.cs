﻿using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BrokeredMessaging.ChatConsole
{
    public class ChatApplication
    {
        private SettingsReader _settingReader;
        static string TopicPath = "chat-topic";
        string ConnectionString = string.Empty;
        public ChatApplication(SettingsReader settingsReader)
        {
            _settingReader = settingsReader;
            ConnectionString = _settingReader.Settings.ConnectionString;
        }


        public void Run()
        {
            Console.WriteLine("Connection String = " + ConnectionString);
            Console.WriteLine("Topic Path = " + TopicPath);
            Console.WriteLine("Enter name:");
            var userName = Console.ReadLine();

            var manager = new ManagementClient(ConnectionString);

            if (!manager.TopicExistsAsync(TopicPath).Result)
            {
                manager.CreateTopicAsync(TopicPath).Wait();
            }

            var description = new SubscriptionDescription(TopicPath, userName)
            {
                AutoDeleteOnIdle = TimeSpan.FromMinutes(5)
            };

            manager.CreateSubscriptionAsync(description).Wait();

            var topicClient = new TopicClient(ConnectionString, TopicPath);
            var subscriptionClient = new SubscriptionClient(ConnectionString, TopicPath, userName);

            // Create a message pump for receiving messages
            subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, ExceptionReceivedHandler);

            var helloMessage = new Message(Encoding.UTF8.GetBytes("Has entered the room..."));
            helloMessage.Label = userName;
            topicClient.SendAsync(helloMessage).Wait();

            while (true)
            {
                string text = Console.ReadLine();
                if (text.Equals("exit")) break;

                // Send a chat message
                var chatMessage = new Message(Encoding.UTF8.GetBytes(text));
                chatMessage.Label = userName;
                topicClient.SendAsync(chatMessage).Wait();
            }

            // Send a message to say you are leaving
            var goodbyeMessage = new Message(Encoding.UTF8.GetBytes("Has left the building..."));
            goodbyeMessage.Label = userName;
            topicClient.SendAsync(goodbyeMessage).Wait();

            // Close the clients
            topicClient.CloseAsync().Wait();
            subscriptionClient.CloseAsync().Wait();
        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Deserialize the message body.
            var text = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine($"{ message.Label }> { text }");
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            return Task.CompletedTask;
        }
    }
}
