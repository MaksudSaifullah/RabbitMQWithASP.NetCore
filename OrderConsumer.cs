using MassTransit;
using MS1.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MS2
{
    public class OrderConsumer :
        IConsumer<Order>
    {
        public async Task Consume(ConsumeContext<Order> context)
        {
            var data = context.Message;
        }
    }
}
