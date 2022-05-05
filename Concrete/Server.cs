using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Linq;
using System.Security.Principal;
using SuvillianceSystem.RabbitMQ_Models.Concrete;
using SuvillianceSystem.RabbitMQAuthorizationService.Infrastructure;

namespace SuvillianceSystem.RabbitMQAuthorizationService
{
    public class ServerMQ
    {
        private IManager ManagerClass { get; set; }
        private ConnectionFactory ConnFactory { get; set; }
        private IConnectorInfo Connector {get;set;}
        

        public ServerMQ(IManager _manager, IConnectorFactoryInfo conectorInfo)
        {
            this.Connector=conectorInfo
            this.ConnFactory = new ConnectionFactory() { HostName = this.Connector.Host };
            ManagerClass = _manager;
        }

        public void ListenLoop()
        {
            using (var connection = this.ConnFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "rpc_queue", durable: false,
                  exclusive: false, autoDelete: false, arguments: null);
                channel.BasicQos(0, 1, false);
                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: "rpc_queue",
                  autoAck: false, consumer: consumer);
                Console.WriteLine(" [x] Awaiting RPC requests");

                consumer.Received += (model, ea) =>
                {
                    string response = null;

                    var body = ea.Body.ToArray();
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;

                    try
                    {
                        var message = Encoding.UTF8.GetString(body);
                        System.Console.WriteLine(".SERVER GOT: {0}", message);
                        AuthDTO authDTO = JsonSerializer.Deserialize<AuthDTO>(message);

                        AuthDTO obj_response = this.ManagerClass.Factory[authDTO.Operation](authDTO);

                        response = JsonSerializer.Serialize(obj_response);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(" [.] " + e.Message);
                        response = "";
                    }
                    finally
                    {
                        System.Console.WriteLine(".SERVER SENT: {0}", response);

                        var responseBytes = Encoding.UTF8.GetBytes(response);
                        channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                          basicProperties: replyProps, body: responseBytes);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag,
                          multiple: false);
                    }
                };

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
