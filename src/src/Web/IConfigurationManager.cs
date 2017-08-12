﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Web
{
    public interface IConfigurationManager
    {
        string NsbEndpointName { get; }

        string NsbEndpointInstanceId { get; }

        string NsbTransportConnectionString { get; }

        string NsbErrorQueueName { get; }

        string NsbAuditQueueName { get; }

        bool NsbIsIntegrationTests { get; }

        DevMode DevMode { get; }
    }
}