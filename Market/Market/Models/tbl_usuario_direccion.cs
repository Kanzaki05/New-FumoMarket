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
    
    public partial class tbl_usuario_direccion
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tbl_usuario_direccion()
        {
            this.tbl_compra = new HashSet<tbl_compra>();
        }
    
        public int ud_id { get; set; }
        public Nullable<int> ud_u_id { get; set; }
        public Nullable<int> ud_direc_id { get; set; }
        public Nullable<bool> ud_activa { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tbl_compra> tbl_compra { get; set; }
        public virtual tbl_direccion tbl_direccion { get; set; }
        public virtual tbl_usuario tbl_usuario { get; set; }
    }
}
