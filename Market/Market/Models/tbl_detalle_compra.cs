//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Market.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tbl_detalle_compra
    {
        public int detalle_id { get; set; }
        public int detalle_fk_compra { get; set; }
        public int detalle_fk_producto { get; set; }
        public int detalle_cantidad { get; set; }
        public decimal detalle_precio_unitario { get; set; }
    
        public virtual tbl_compra tbl_compra { get; set; }
        public virtual tbl_producto tbl_producto { get; set; }
    }
}
