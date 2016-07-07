using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Foundatio.RabbitMq.Data {
    public interface ISimpleMessage {
        string Data { get; set; }
    }

    public class SimpleMessageA : ISimpleMessage {
        public string Data { get; set; }
        public int Count { get; set; }
    }
}
