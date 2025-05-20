using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using MessagePack;

public class Client
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly string _responseQueueName;

    public Client()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Deklaracja kolejki odpowiedzi (tymczasowa)
        _responseQueueName = _channel.QueueDeclare().QueueName;
    }

    // Wysyłanie zadania do serwera
    public void SendTask(float[] matrixA, float[] matrixB, ProcessingType processingType)
    {
        var task = new ProcessingTask
        {
            TaskId = Guid.NewGuid().ToString(),
            MatrixA = matrixA,
            MatrixB = matrixB,
            ProcessingType = processingType
        };

        // Serializacja MessagePack
        var body = MessagePackSerializer.Serialize(task);

        // Wysłanie do kolejki "request_queue"
        _channel.BasicPublish(
            exchange: "",
            routingKey: "request_queue",
            basicProperties: null,
            body: body
        );

        Console.WriteLine($"Wysłano zadanie: {task.TaskId}");
    }

    // Odbieranie wyników
    public void ListenForResults()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var result = MessagePackSerializer.Deserialize<ProcessingResult>(body);
            Console.WriteLine($"Odebrano wynik: {result.TaskId}, Wynik: {result.Result[0]}...");
        };

        _channel.BasicConsume(
            queue: _responseQueueName,
            autoAck: true,
            consumer: consumer
        );
    }
}

// Klasy pomocnicze
[MessagePackObject]
public class ProcessingTask
{
    [Key(0)] public string TaskId { get; set; }
    [Key(1)] public float[] MatrixA { get; set; }
    [Key(2)] public float[] MatrixB { get; set; }
    [Key(3)] public ProcessingType ProcessingType { get; set; }
}

[MessagePackObject]
public class ProcessingResult
{
    [Key(0)] public string TaskId { get; set; }
    [Key(1)] public float[] Result { get; set; }
}

public enum ProcessingType { CpuSingleThread, CpuMultiThread, Gpu, GpuWithPooling }
