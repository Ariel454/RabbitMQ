using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ
{
    public class Alerta
    {
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public string Detalle { get; set; }
        public DateTime HoraDetectada { get; set; }

        public Alerta(string nombre, string codigo, string detalle)
        {
            Nombre = nombre;
            Codigo = codigo;
            Detalle = detalle;
            HoraDetectada = DateTime.Now; // La hora actual al momento de crear la alerta
        }
    }
}
