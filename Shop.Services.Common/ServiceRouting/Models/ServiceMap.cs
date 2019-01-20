using System.Collections.Generic;

namespace Shop.Services.Common.ServiceRouting.Models
{
    public class ServiceMap
    {
        public IDictionary<string, Service> Services { get; set; }
    }
}