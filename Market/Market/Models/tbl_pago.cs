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
    
    public partial class tbl_pago
    {
        public int pago_id { get; set; }
        public string pago_tipo { get; set; }
        public string pago_tarjeta { get; set; }
        public string pago_caducidad { get; set; }
        public string pago_cvv { get; set; }
        public int pago_fk_compra { get; set; }
        public Nullable<System.DateTime> pago_fecha { get; set; }
    
        public virtual tbl_compra tbl_compra { get; set; }
    }
}
