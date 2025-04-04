using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Market.Models
{
    public class VerPedidoModel
    {
        // Número de Pedido
        public int NumeroPedido { get; set; }

        // Dirección
        public string Pais { get; set; }
        public string Provincia { get; set; }
        public string Distrito { get; set; }
        public string CodigoPostal { get; set; }
        public string Linea1 { get; set; }
        public string Linea2 { get; set; }
        public DateTime? FechaCompra { get; set; }
        public string MetodoPago { get; set; }
        public decimal PrecioTotal { get; set; }
        public List<ProductoPedido> Productos { get; set; }
    }

    public class ProductoPedido
    {
        public string ProductoNombre { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioTotal { get; set; }
        public string Imagen { get; set; }

    }
}
