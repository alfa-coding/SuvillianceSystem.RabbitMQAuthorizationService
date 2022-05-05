using System;
using System.Collections.Generic;
using SuvillianceSystem.RabbitMQ_Models.Concrete;

namespace SuvillianceSystem.RabbitMQAuthorizationService.Infrastructure
{
    public interface IConnectorFactoryInfo
    {
        string Host {get;set;}
    }
}