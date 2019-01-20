using System.Collections.Generic;

namespace Shop.Services.Common.ServiceRouting.Models
{
    public class Queue
    {
        public string Name { get; set; }

        public IEnumerable<string> Commands { get; set; }

        public IEnumerable<string> SagaMessages { get; set; }
    }
}