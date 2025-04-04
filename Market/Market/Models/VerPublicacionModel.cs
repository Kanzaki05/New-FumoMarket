using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Market.Models
{
    public class VerPublicacion
    {

        public int pro_id { get; set; }
        public string pro_nombre { get; set; }
        public string pro_imagen { get; set; }
        public string pro_descrip { get; set; }
        public Nullable<int> pro_precio { get; set; }
        public Nullable<int> pro_fk_cat { get; set; }
        public Nullable<int> pro_fk_user { get; set; }
        public int cat_id { get; set; }
        public string cat_nombre { get; set; }
        public string u_nombre { get; set; }
        public string u_imagen { get; set; }
        public string u_contacto { get; set; }

    }
}