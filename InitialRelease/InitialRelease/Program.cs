﻿using Azure.Messaging;
using Azure.Messaging.ServiceBus;

string connectionString = "<connection_string>";
string queueName = "<queue_name>";

// since ServiceBusClient implements IAsyncDisposable we create it with "await using"
await using var client = new ServiceBusClient(connectionString);

// create the sender
ServiceBusSender sender = client.CreateSender(queueName);

// create a payload using the CloudEvent type
var cloudEvent = new CloudEvent(
    "/cloudevents/example/source",
    "Example.Employee",
    new Employee { Name = "Homer", Age = 39 });
ServiceBusMessage message = new ServiceBusMessage(new BinaryData(cloudEvent))
{
    ContentType = "application/cloudevents+json"
};

// send the message
await sender.SendMessageAsync(message);

// create a receiver that we can use to receive and settle the message
ServiceBusReceiver receiver = client.CreateReceiver(queueName);

// receive the message
ServiceBusReceivedMessage receivedMessage = await receiver.ReceiveMessageAsync();

// deserialize the message body into a CloudEvent
CloudEvent receivedCloudEvent = CloudEvent.Parse(receivedMessage.Body);

// deserialize to our Employee model
Employee receivedEmployee = receivedCloudEvent.Data.ToObjectFromJson<Employee>();

// prints 'Homer'
Console.WriteLine(receivedEmployee.Name);

// prints '39'
Console.WriteLine(receivedEmployee.Age);

// complete the message, thereby deleting it from the service
await receiver.CompleteMessageAsync(receivedMessage);
