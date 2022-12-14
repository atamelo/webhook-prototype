// See https://aka.ms/new-console-template for more information
using Azure;
using Azure.Identity;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using NUnit.Framework;

QueueClient queue = new(
    "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;", 
    "dispatch"
);
queue.Create();
await queue.SendMessageAsync("first");
await queue.SendMessageAsync("second");
await queue.SendMessageAsync("third");

// Get the messages from the queue with a short visibility timeout
// of 1 second
List<QueueMessage> messages = new List<QueueMessage>();
Response<QueueMessage[]> received = await queue.ReceiveMessagesAsync(10, TimeSpan.FromSeconds(1));
foreach (QueueMessage message in received.Value)
{
    // Tell the service we need a little more time to process the
    // message by giving them a 5 second visiblity window
    UpdateReceipt receipt = await queue.UpdateMessageAsync(
        message.MessageId,
        message.PopReceipt,
        message.Body,
        TimeSpan.FromSeconds(5));

    // Keep track of the updated messages
    messages.Add(message.Update(receipt));
}

// Wait until the original 1 second visiblity window times out and
// check to make sure the messages aren't showing up yet
await Task.Delay(TimeSpan.FromSeconds(1.5));
Assert.AreEqual(0, (await queue.ReceiveMessagesAsync(10)).Value.Length);

// Finish processing the messages
foreach (QueueMessage message in messages)
{
    // "Process" the message
    Console.WriteLine($"Message: {message.Body}");

    // Tell the service we need a little more time to process the message
    await queue.DeleteMessageAsync(message.MessageId, message.PopReceipt);
}