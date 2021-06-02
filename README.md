
1.	What is RabbitMQ?

RabbitMQ is the most widely deployed open source message broker. It supports various messaging protocols. It basically gives our applications a common platform for sending and receiving messages. This ensures that our message (data) is never lost and is successfully received by each intended consumer.  RabbitMQ makes the entire process seamless.

Ina simple words, we will have a publisher that publishes messages to the message broker (RabbitMQ server). Now the server stores the message in a queue. To this particular queue, multiple consumers can subscribe. Whenever there is a new message, each of the subscribers would receive it. An application can act as both producer / consumer based on how you configure it and what the requirement demands.

Apart from that, RabbitMQ provides a cool Dashboard for monitoring the messages and queues. We will be setting up this too later in this article!

2.	Advantages of RabbitMQ
There are quite a lot of advantages of using a queue based messaging solution rather than directly sending messages to the intended consumer. Here are few of the advantages.
•	Better Scalability – Now, you will not have to depend on just one VM / processor / server to process your request. When it gets to the point where you first server finds it tough to process the incoming queue data, you can simply add another server that can share the load and improve the overall response time. This is quite easy with the queue concept in RabbitMQ.


•	Clean User Experience – You users are less likely to see any errors, thanks to the microservice based message broker architecture.
•	Higher availability – Even if the main Microservice is down to a technical glitch on on-going update, the messages are never lost. It gets stored to the RabbitMQ server. Once the Service comes online, it consumes the pending messages and processes it.
 ![image](https://user-images.githubusercontent.com/83159793/120458135-d8956400-c3b8-11eb-83d8-d2f206439129.png)

Fig1: Micoservice communication via RabbitMQ
3.	Setting up the Environment
•	We will be working with ASP.NET Core 3.1 WebAPI using Visual Studio 2019 IDE. Make sure you have them up and running with the latest SDK.
•	After that, we will need to setup the RabbitMQ server and dashboard.
3.1	Installing ErLang
Erlang is a programming language with which the RabbitMQ server is built on. Since we are installing the RabbitMQ Server locally to our Machine (Windows 10), make sure that you install Erlang first. Download the Installer from here – https://www.erlang.org/downloads/24.0 . At the time of writing, the latest available version of Erlang is 24.0. Install it in your machine with Administrator Rights.

3.2	Installing RabbitMQ as a service in windows
We will be installing the RabbitMQ Server and a service within our Windows machine. Make sure that RabbitMQ Latest compatible with erlang
https://www.rabbitmq.com/install-windows.html
3.3	In case of failing to create cookie file (drive:)/ErLang
1.	cd C:\Program Files\RabbitMQ Server\rabbitmq_server-3.8.7\sbin
2.	rabbitmq-plugins enable rabbitmq_management
3.	net stop RabbitMQ
4.	net start RabbitMQ

Line #1 we make the installation directory as our default working directory for cmd. Please note that your directory path may differ.
Line #2 – Here we enable the Management Plugin
Line #3-4 – We restart the RabbitMQ Service.
 
Fig2: RabbitMQ server activation by command prompt.

That’s it. Now Navigate to http://localhost:15672/. Here is where you can find the   management dashboard of RabbitMQ running. Default username & password is guest

Fig3: RabbitMQ server

![image](https://user-images.githubusercontent.com/83159793/120458316-febb0400-c3b8-11eb-9e51-5f74bc81b15d.png)

Fig3: RabbitMQ server login

![image](https://user-images.githubusercontent.com/83159793/120458358-07abd580-c3b9-11eb-8c15-8a5f5f4b4b4d.png)

Fig4: RabbitMQ Dashboard

![image](https://user-images.githubusercontent.com/83159793/120458409-13979780-c3b9-11eb-8e4b-2dc6005fe601.png)

Fig4: RabbitMQ server 
	
4. Getting Started – RabbitMQ with ASP.NET Core
Now that our server is configured, let’s build the actual microservices that can interact with each other via RabbitMQ. Before proceeding, I highly recommend you to go through Microservice Architecture in ASP.NET Core to get a basic idea on how Microservice Architecture works. 
So, I created a new Blank solution ‘MS1’. Here we will be adding 2 Microservices
•	MS1
•	MS2
4.1	Inside MS1 class library do the following steps
Add references from nuget 
MassTransit (7.1.8)
MassTransit.RabbitMQ (7.1.8)
MassTransit.AspNetCore(7.1.8)
 
![image](https://user-images.githubusercontent.com/83159793/120458458-1d20ff80-c3b9-11eb-8cc6-389cb60a9e55.png)

	Fig: Installed packages


4.2	Add OrderController.cs class inside Controllers Folder with following code snippets


    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IBusControl _bus;

        public OrderController(IBusControl bus)
        {
            _bus = bus;
        }
        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            Uri uri = new Uri("rabbitmq://localhost/order-queue");

            var endPoint = await _bus.GetSendEndpoint(uri);
            await endPoint.Send(order);
            return Ok("Success");
        }
     }
Note: Here we name our service name as order-queue. We can name it what we want.
4.3	Add Order.cs class inside ViewModel Folder with following code snippets

   public class Order
    {
        public int id { get; set; }
        public string OrderName { get; set; }
        public int OrderQty { get; set; }
    }

4.4	Change the Startup.cs class by the following code snippets

  public void ConfigureServices(IServiceCollection services)
        {
            services.AddMassTransit(x =>
            {
                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    // configure health checks for this bus instance
                    cfg.UseHealthCheck(provider);

                    cfg.Host("rabbitmq://localhost");
                }));
            });

            services.AddMassTransitHostedService();

            services.AddControllers();
        }

4.5 Inside MS2 class library do the following steps
	In order to reduce time, go to MS-1 and right click -> edit project file and copy the following lines and paste to MS-2 by editing project file:
  
<ItemGroup>
    <PackageReference Include="MassTransit" Version="7.1.8" />
    <PackageReference Include="MassTransit.AspNetCore" Version="7.1.8" />
    <PackageReference Include="MassTransit.RabbitMQ" Version="7.1.8" />
</ItemGroup>

4.6	Inside MS2 class library do the following steps add Order.cs class inside ViewModel Folder with following code snippets

    public class Order
    {
        public int id { get; set; }
        public string OrderName { get; set; }
        public int OrderQty { get; set; }
    }

4.7	Create OrderConsumer.cs class and add these following lines to consume message from MS-1 via RabbitMQ

public class OrderConsumer : IConsumer<Order>
   {
        public async Task Consume(ConsumeContext<Order> context)
        {
            var data = context.Message;
        }
    }
  
 4.8 Change the Startup.cs class by the following code snippets
  
    public void ConfigureServices(IServiceCollection services)
        	{
            		services.AddMassTransit(x =>
            		{
                		x.AddConsumer<OrderConsumer>();

                		x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
               		 {
                   		 // configure health checks for this bus instance
                   		 cfg.UseHealthCheck(provider);

                   		 cfg.Host("rabbitmq://localhost");

                    		cfg.ReceiveEndpoint("order-queue", ep =>
                    		{
                        		ep.PrefetchCount = 16;
                        		ep.UseMessageRetry(r => r.Interval(2, 100));

                        		ep.ConfigureConsumer<OrderConsumer>(provider);
                    		});
               	 }));
            	});

            		services.AddMassTransitHostedService();

            		services.AddControllers();
   }

5.	Testing Phase
5.1 Scenario #1 – When the OrderConsumer is Online
Now we are all set to test our application. Before run this application change the following configuration as follows. That’s it for our first Microservice, the publisher. Let’s build it and run. We will POST data to the order endpoint using POSTMAN. Here is the POST request we will pass to the order endpoint.
  ![image](https://user-images.githubusercontent.com/83159793/120458569-33c75680-c3b9-11eb-9349-f06bb74db7c5.png)

 
First, we will have both the services online, and try to pass some sample data. I put a breakpoint at the Consumer Class and also at the OrderController to verify the received data from POSTMAN. Let’s run the Applications and switch back to POSTMAN.

 
![image](https://user-images.githubusercontent.com/83159793/120458621-3f1a8200-c3b9-11eb-900f-03d51560beef.png)


This is how our POST request looks like. If things go well, we should have a breakpoint hit at the OrderController. Let’s see the data. 


![image](https://user-images.githubusercontent.com/83159793/120458646-43df3600-c3b9-11eb-9778-9dfb5357b595.png)


You can see that we are able to pass the Model to the First Microservice – MS-1. Let’s continue the debug and switch back to our RabbitMQ Dashboard.
 ![Uploading sd.png…]()
  
<img width="955" alt="dd" src="https://user-images.githubusercontent.com/83159793/120459638-278fc900-c3ba-11eb-8c2f-b2a359f19b51.png">


You can see the we have one un-Processed message. Also, we will be hitting our next breakpoint at the second Microservice MS-2 as well. Let’s verify the data.
5.2 Scenario #2 – When the OrderConsumer is Offline and back after N moments
In this case, a more practical scenario where the consumer can be offline due to several reasons. When the consumer is offline, the publisher can still send the message to the RabbitMQ server queue. As soon as the consumer comes online, it should be able to consume the pending message. That’s the whole point of Message Broking right? Let’s check.
So, I disable the Startup of the Consumer Project. Now only the MS-1 will run. This mimics the scenario where the consumer if offline.
Let’s POST the message via POSTMAN to the publisher. We will get back a 200 Ok Response as expected. Let’s check the RabbitMQ Dashboard.

![image](https://user-images.githubusercontent.com/83159793/120459089-ae907180-c3b9-11eb-9508-a6d95cc9cffc.png)
![image](https://user-images.githubusercontent.com/83159793/120459117-b6501600-c3b9-11eb-97eb-1c8d0bebfb15.png)
![image](https://user-images.githubusercontent.com/83159793/120459140-ba7c3380-c3b9-11eb-8e7f-0d123d5b89ae.png)
 
At the moment our MS-2 restored it will consume the services from RabbitMQ
![image](https://user-images.githubusercontent.com/83159793/120459156-bea85100-c3b9-11eb-8b88-dad471fc171e.png)
That’s a wrap for this article!

Summary
In this article, we have gone through Message Brokers, RabbitMQ, Advantages, Integrating RabbitMQ with ASP.NET Core using MassTransit. We also build a small prototype application to send data over the RabbitMQ Server. You can find the .

