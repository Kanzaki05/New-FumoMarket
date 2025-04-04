using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Market.Models
{
    public class VerUsuario
    {
        public int u_id { get; set; }
        public string u_nombre { get; set; }
        public string u_email { get; set; }
        public string u_contacto { get; set; }
        public string u_imagen { get; set; }
        public string u_descrip { get; set; }
        public List<VerPublicacion> Productos { get; set; }
        public List<VerPedidoModel> Pedidos { get; set; }
    }

}