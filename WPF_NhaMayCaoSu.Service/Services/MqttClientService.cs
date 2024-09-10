﻿using MQTTnet.Client;
using MQTTnet.Protocol;
using MQTTnet;
using System.Text;
using WPF_NhaMayCaoSu.Service.Interfaces;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using MQTTnet.Adapter;


namespace WPF_NhaMayCaoSu.Service.Services
{
    public class MqttClientService : IMqttClientService
    {
        private readonly IMqttClient _client;
        private readonly MqttClientOptions _options;

        public event EventHandler<string> MessageReceived;
        public bool IsConnected => _client?.IsConnected ?? false;


        public MqttClientService()
        {
            MqttFactory factory = new MqttFactory();
            _client = factory.CreateMqttClient();

            MqttClientOptionsBuilder optionsBuilder = new MqttClientOptionsBuilder();
            string ipLocal = GetLocalIpAddress();
            optionsBuilder.WithClientId("this_computer")
                           .WithTcpServer($"{ipLocal}", 1883)
                           .WithCredentials("admin", "admin")
                           .WithCleanSession();

            _options = optionsBuilder.Build();

            // Register event handlers
            _client.ConnectedAsync += OnConnectedAsync;
            _client.DisconnectedAsync += OnDisconnectedAsync;
            _client.ApplicationMessageReceivedAsync += OnMessageReceivedAsync;
        }

        private string GetLocalIpAddress()
        {
            System.Net.IPAddress[] ipAddresses = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());

            foreach (System.Net.IPAddress ip in ipAddresses)
            {
                // Check for IPv4 addresses
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "N/A";
        }

        private Task OnConnectedAsync(MqttClientConnectedEventArgs arg)
        {
            Debug.WriteLine("Connected to MQTT broker.");
            return Task.CompletedTask;
        }

        private Task OnDisconnectedAsync(MqttClientDisconnectedEventArgs arg)
        {
            Debug.WriteLine("Disconnected from MQTT broker.");
            return Task.CompletedTask;
        }

        private Task OnMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs arg)
        {
            string message = Encoding.UTF8.GetString(arg.ApplicationMessage.Payload);
            Console.WriteLine($"Message received on topic {arg.ApplicationMessage.Topic}: {message}");

            // Phân tích tin nhắn JSON
            var jsonMessage = JsonConvert.DeserializeObject<JObject>(message);
            string rfid = jsonMessage["RFID"]?.ToString();
            string density = jsonMessage["Density"]?.ToString();
            string weight = jsonMessage["Weight"]?.ToString();
            string macAddress = jsonMessage["MACAddress"]?.ToString();
            string currentMode = jsonMessage["Mode"]?.ToString();

            // Kiểm tra topic và xử lý tin nhắn tương ứng
            switch (arg.ApplicationMessage.Topic)
            {
                case "CreateRFID":
                    if (!string.IsNullOrEmpty(rfid))
                    {
                        MessageReceived?.Invoke(this, $"CreateRFID:{rfid}");
                    }
                    break;

                case "Can_tieu_ly":
                    if (!string.IsNullOrEmpty(rfid) && !string.IsNullOrEmpty(density))
                    {
                        MessageReceived?.Invoke(this, $"Can_tieu_ly:{rfid}:{density}");
                    }
                    break;

                case "Can_ta":
                    if (!string.IsNullOrEmpty(rfid) && !string.IsNullOrEmpty(weight))
                    {
                        MessageReceived?.Invoke(this, $"Can_ta:{rfid}:{weight}");
                    }
                    break;
                case "Canta_info":
                    if (!string.IsNullOrEmpty(macAddress) && !string.IsNullOrEmpty(currentMode))
                    {
                        MessageReceived?.Invoke(this, $"CantaInfo:{macAddress}:{currentMode}");
                    }
                    break;
                case "Cantieuly_info":
                    if (!string.IsNullOrEmpty(macAddress) && !string.IsNullOrEmpty(currentMode))
                    {
                        MessageReceived?.Invoke(this, $"CantieuLyInfo:{macAddress}:{currentMode}");
                    }
                    break;
            }

            return Task.CompletedTask;
        }



        public async Task ConnectAsync()
        {
            if (!_client.IsConnected)
            {
                try
                {
                    await _client.ConnectAsync(_options);
                }
                catch (MqttConnectingFailedException ex)
                {
                    Debug.WriteLine("Kết nối đến máy chủ MQTT thất bại: " + ex.Message);
                    throw new Exception("Kết nối đến máy chủ MQTT thất bại. Vui lòng đảm bảo rằng máy chủ đã được bật.", ex);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Đã xảy ra lỗi khi kết nối đến máy chủ MQTT: " + ex.Message);
                    throw new Exception("Đã xảy ra lỗi khi kết nối đến máy chủ MQTT. Vui lòng đảm bảo rằng máy chủ đã được bật.", ex);
                }
            }
        }



        public async Task SubscribeAsync(string topic)
        {
            MqttTopicFilterBuilder topicFilterBuilder = new MqttTopicFilterBuilder();
            topicFilterBuilder.WithTopic(topic);

            await _client.SubscribeAsync(topicFilterBuilder.Build());
        }

        public async Task PublishAsync(string topic, string payload)
        {
            if (!_client.IsConnected)
            {
                try
                {
                    await _client.ConnectAsync(_options);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Failed to connect to MQTT broker before publishing: " + ex.Message);
                    throw; 
                }
            }

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .WithRetainFlag()
                .Build();

            await _client.PublishAsync(message);
        }

        public async Task CloseConnectionAsync()
        {
            if (_client.IsConnected)
            {
                await _client.DisconnectAsync();
                Console.WriteLine("Disconnected from MQTT broker.");
            }
        }
    }
}
