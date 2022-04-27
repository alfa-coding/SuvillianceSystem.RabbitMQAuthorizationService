using System;
using System.Collections.Generic;
using SuvillianceSystem.RabbitMQ_Models.Concrete;

namespace SuvillianceSystem.RabbitMQAuthorizationService.Infrastructure
{
    public interface IManager
    {
        Dictionary<OperationType, Func<AuthDTO, AuthDTO>> Factory { get; set; }
    }
}