using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using MessagePack;

public class Server
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public Server()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Deklaracja kolejki "request_queue"
        _channel.QueueDeclare(queue: "request_queue", durable: true, exclusive: false);
    }

    public void StartProcessing()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var task = MessagePackSerializer.Deserialize<ProcessingTask>(body);

            // Przetwarzanie danych (CPU/GPU)
            float[] result;
            switch (task.ProcessingType)
            {
                case ProcessingType.Gpu:
                case ProcessingType.GpuWithPooling:
                    result = ProcessOnGpu(task.MatrixA, task.MatrixB); // ← Tutaj dodać integrację z CUDA
                    break;
                default:
                    result = ProcessOnCpu(task.MatrixA, task.MatrixB);
                    break;
            }

            // Wysłanie wyniku do kolejki "response_queue"
            var resultMessage = new ProcessingResult { TaskId = task.TaskId, Result = result };
            var responseBody = MessagePackSerializer.Serialize(resultMessage);

            _channel.BasicPublish(
                exchange: "",
                routingKey: "response_queue",
                basicProperties: null,
                body: responseBody
            );
        };

        _channel.BasicConsume(queue: "request_queue", autoAck: true, consumer: consumer);
    }

    // Przykładowe metody przetwarzania
    private float[] ProcessOnCpu(float[] a, float[] b)
    {
        // Symulacja obliczeń na CPU (np. mnożenie macierzy)
        var result = new float[a.Length];
        Parallel.For(0, a.Length, i => result[i] = a[i] * b[i]); // Wersja wielowątkowa
        return result;
    }

    private float[] ProcessOnGpu(float[] a, float[] b)
    {
        // TODO: Integracja z CUDA (wymaga biblioteki np. ManagedCuda)
        // Przykład:
        // using var kernel = new CudaKernel("multiply.ptx");
        // kernel.Run(a, b, result);
        return new float[a.Length]; // Symulacja
    }
}
