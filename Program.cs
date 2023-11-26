using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using RabbitMQ;
using System.Net.Mail;
using System.Net;

class Program
{
    static void Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            // Configurar consumidores
            ConfigureVulnerabilitiesConsumer(channel);
            ConfigureStadisticsConsumer(channel);

            // Configuración del productor
            ConfigureProducer(channel);

            Console.WriteLine($"[*] Esperando eventos con la clave de enrutamiento 'notificaciones'. Presiona CTRL+C para salir.");

            Console.ReadLine(); 
        }
    }

    static void ConfigureVulnerabilitiesConsumer(IModel channel)
    {
        var vulnerabilitiesConsumer = new EventingBasicConsumer(channel);
        vulnerabilitiesConsumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var mensaje = Encoding.UTF8.GetString(body);

            Console.WriteLine($"[x] Recibido en vulnerabilities_log: '{mensaje}' con la clave de enrutamiento '{ea.RoutingKey}'");
            SendEmail("arielsebastiandiaz454@gmail.com", "Nueva alerta", mensaje);
        };

        // Consumir de la cola
        channel.BasicConsume(queue: "vulnerabilities_log", autoAck: true, consumer: vulnerabilitiesConsumer);
    }

    static void ConfigureStadisticsConsumer(IModel channel)
    {
        var stadisticsConsumer = new EventingBasicConsumer(channel);
        stadisticsConsumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var mensaje = Encoding.UTF8.GetString(body);

            Console.WriteLine($"[x] Recibido en stadistics_log: '{mensaje}' con la clave de enrutamiento '{ea.RoutingKey}'");
            SendEmail("arielsebastiandiaz454@gmail.com", "Nueva alerta", mensaje);
        };

        // Consumir de la cola
        channel.BasicConsume(queue: "stadistics_log", autoAck: true, consumer: stadisticsConsumer);
    }

    static void SendEmail(string to, string subject, string message)
    {
        try
        {
            var smtpClient = new SmtpClient("sandbox.smtp.mailtrap.io")
            {
                Port = 587,
                Credentials = new NetworkCredential("9eba10672c4457", "23836081ca86b8"),
                EnableSsl = true,
                UseDefaultCredentials = false,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("arielsebastiandiaz454@gmail.com", "Ariel Raura"),
                Subject = subject,
                Body = message,
            };

            mailMessage.To.Add(to);

            smtpClient.Send(mailMessage);

            Console.WriteLine($"Correo electrónico enviado a {to}: {message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar el correo electrónico: {ex.Message}");
        }
    }
    static void ConfigureProducer(IModel channel)
    {
        string routingKey = "notificaciones";

        while (true)
        {
            Console.WriteLine("Ingrese los detalles de la alerta:");

            Console.Write("Nombre: ");
            string nombre = Console.ReadLine();

            Console.Write("Código: ");
            string codigo = Console.ReadLine();

            Console.Write("Detalle: ");
            string detalle = Console.ReadLine();

            Alerta nuevaAlerta = new Alerta(nombre, codigo, detalle);
            string mensaje = Newtonsoft.Json.JsonConvert.SerializeObject(nuevaAlerta);

            var body = Encoding.UTF8.GetBytes(mensaje);

            channel.BasicPublish(exchange: "testTopic", routingKey: routingKey, basicProperties: null, body: body);

            Console.WriteLine($"[x] Enviado '{mensaje}' con la clave de enrutamiento '{routingKey}'");

            Console.WriteLine("Presiona Enter para enviar otra alerta o escribe 'exit' y presiona Enter para salir.");

            string decision = Console.ReadLine();

            if (decision.ToLower() == "exit")
            {
                break; 
            }
        }
    }


}
