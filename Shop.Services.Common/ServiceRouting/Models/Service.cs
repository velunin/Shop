namespace Shop.Services.Common.ServiceRouting.Models
{
    public class Service
    {
        public string Name { get; set; }

        public Queue[] Queues { get; set; }

        public Path[] Paths { get; set; }
    }
}