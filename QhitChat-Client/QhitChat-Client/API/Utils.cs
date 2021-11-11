﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QhitChat_Client.API
{
    class Utils
    {
        public async static Task<string> PingAsync()
        {
            return await Core.Configuration.Network.InvokeAsync<string>("Ping");
        }
    }
}