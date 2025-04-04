using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Market.Models
{
    public class VerUsuarioConPedidos
    {
        public VerUsuario Usuario { get; set; }
        public List<VerPedidoModel> Pedidos { get; set; }
    }
}