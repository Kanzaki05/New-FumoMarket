using Market.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;


namespace Market.Controllers
{
    public class UsuarioController : Controller
    {
        dbMarketplaceEntities db = new dbMarketplaceEntities();
        // GET: User
        public ActionResult Index(int? page)
        {
            int pagesize = 9, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;

            // Ordenar alfabéticamente por el nombre de la categoría
            var list = db.tbl_categoria
                         .Where(x => x.cat_estado == 1)
                         .OrderBy(x => x.cat_nombre)  // Orden alfabético ascendente
                         .ToList();

            IPagedList<tbl_categoria> stu = list.ToPagedList(pageindex, pagesize);

            return View(stu);
        }

        public ActionResult Registrar()
        {

            return View();
        }

		public ActionResult MiPerfil()
		{
			// Verifica si el usuario está autenticado
			if (Session["u_id"] == null)
			{
				return RedirectToAction("Login");
			}

			int userId = Convert.ToInt32(Session["u_id"]);

			// Obtén los datos del usuario
			var usuario = db.tbl_usuario.FirstOrDefault(u => u.u_id == userId);
			if (usuario == null)
			{
				return HttpNotFound();
			}

			// Obtén las publicaciones del usuario
			var publicaciones = db.tbl_producto.Where(p => p.pro_fk_user == userId).ToList();

			// Convertir las publicaciones de tbl_producto a VerPublicacion
			var publicacionesVerUsuario = publicaciones.Select(p => new VerPublicacion
			{
                pro_imagen = p.pro_imagen,
				pro_id = p.pro_id,
				pro_nombre = p.pro_nombre,
				pro_descrip = p.pro_descrip,
                pro_precio = p.pro_precio,
				
			}).ToList();

			// Crea el modelo
			var model = new VerUsuario
			{
				u_id = usuario.u_id,
				u_nombre = usuario.u_nombre,
				u_email = usuario.u_email,
				u_imagen = usuario.u_imagen ?? "~/Content/default-user.png",
				u_contacto = usuario.u_contacto,
                u_descrip = usuario.u_descrip,
				Productos = publicacionesVerUsuario  // Asigna la lista convertida
			};

			return View(model);
		}

        // Acción para ver el perfil del usuario y sus pedidos
        public ActionResult MisPedidos(int id)
        {
            // Verifica si el usuario está autenticado
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login");
            }

            int userId = Convert.ToInt32(Session["u_id"]);

            // Obtén los datos del usuario
            var usuario = db.tbl_usuario.FirstOrDefault(u => u.u_id == userId);
            if (usuario == null)
            {
                return HttpNotFound();
            }

            // Mapea el usuario a un objeto VerUsuario
            var verUsuario = new VerUsuario
            {   
                u_imagen = usuario.u_imagen,
                u_id = usuario.u_id,
                u_nombre = usuario.u_nombre,
                u_email = usuario.u_email,
                u_contacto = usuario.u_contacto,
                u_descrip=usuario.u_descrip,
                // Mapea otros campos si es necesario
            };

            // Obtener los pedidos del usuario
            var pedidos = (from c in db.tbl_compra
                           join ud in db.tbl_usuario_direccion on c.compra_fk_direccion equals ud.ud_id
                           join d in db.tbl_direccion on ud.ud_direc_id equals d.direc_id
                           join p in db.tbl_detalle_compra on c.compra_id equals p.detalle_fk_compra
                           join prod in db.tbl_producto on p.detalle_fk_producto equals prod.pro_id
                           where c.compra_fk_usuario == userId
                           group new { c, d, p, prod } by new
                           {
                               c.compra_id,
                               d.direc_pais,
                               d.direc_provincia,
                               d.direc_distrito,
                               d.direc_postal,
                               d.direc_linea1,
                               d.direc_linea2,
                               c.compra_fecha,
                               c.compra_metodo_pago,
                               c.compra_total
                           } into g
                           select new VerPedidoModel
                           {
                               NumeroPedido = g.Key.compra_id,
                               Pais = g.Key.direc_pais,
                               Provincia = g.Key.direc_provincia,
                               Distrito = g.Key.direc_distrito,
                               CodigoPostal = g.Key.direc_postal,
                               Linea1 = g.Key.direc_linea1,
                               Linea2 = g.Key.direc_linea2,
                               FechaCompra = g.Key.compra_fecha,
                               MetodoPago = g.Key.compra_metodo_pago,
                               PrecioTotal = g.Key.compra_total,
                               Productos = g.Select(x => new ProductoPedido
                               {
                                   ProductoNombre = x.prod.pro_nombre,
                                   Cantidad = x.p.detalle_cantidad,
                                   PrecioUnitario = x.p.detalle_precio_unitario,
                                   PrecioTotal = x.p.detalle_precio_unitario * x.p.detalle_cantidad
                               }).ToList()
                           }).ToList();

            // Crear el modelo que contiene tanto al usuario como los pedidos
            var modelo = new VerUsuarioConPedidos
            {
                Usuario = verUsuario,
                Pedidos = pedidos
            };

            // Si no hay pedidos
            if (!pedidos.Any())
            {
                ViewBag.Message = "No tienes pedidos aún.";
            }

            return View(modelo); // Cambia aquí para pasar el objeto 'modelo'
        }

        public ActionResult VerPedido(int id)
        {
            // Verifica si el usuario está autenticado
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login");
            }

            int userId = Convert.ToInt32(Session["u_id"]);

            // Obtén los datos del usuario
            var usuario = db.tbl_usuario.FirstOrDefault(u => u.u_id == userId);
            if (usuario == null)
            {
                return HttpNotFound();
            }

            // Mapea el usuario a un objeto VerUsuario
            var verUsuario = new VerUsuario
            {
                u_imagen = usuario.u_imagen,
                u_id = usuario.u_id,
                u_nombre = usuario.u_nombre,
                u_email = usuario.u_email,
                u_contacto = usuario.u_contacto,
                u_descrip = usuario.u_descrip,
                // Mapea otros campos si es necesario
            };

            // Obtener el pedido específico por su ID
            var pedido = (from c in db.tbl_compra
                          join ud in db.tbl_usuario_direccion on c.compra_fk_direccion equals ud.ud_id
                          join d in db.tbl_direccion on ud.ud_direc_id equals d.direc_id
                          join p in db.tbl_detalle_compra on c.compra_id equals p.detalle_fk_compra
                          join prod in db.tbl_producto on p.detalle_fk_producto equals prod.pro_id
                          where c.compra_fk_usuario == userId && c.compra_id == id
                          group new { c, d, p, prod } by new
                          {
                              c.compra_id,
                              d.direc_pais,
                              d.direc_provincia,
                              d.direc_distrito,
                              d.direc_postal,
                              d.direc_linea1,
                              d.direc_linea2,
                              c.compra_fecha,
                              c.compra_metodo_pago,
                              c.compra_total
                          } into g
                          select new VerPedidoModel
                          {
                              NumeroPedido = g.Key.compra_id,
                              Pais = g.Key.direc_pais,
                              Provincia = g.Key.direc_provincia,
                              Distrito = g.Key.direc_distrito,
                              CodigoPostal = g.Key.direc_postal,
                              Linea1 = g.Key.direc_linea1,
                              Linea2 = g.Key.direc_linea2,
                              FechaCompra = g.Key.compra_fecha,
                              MetodoPago = g.Key.compra_metodo_pago,
                              PrecioTotal = g.Key.compra_total,
                              Productos = g.Select(x => new ProductoPedido
                              {
                                  ProductoNombre = x.prod.pro_nombre,
                                  Cantidad = x.p.detalle_cantidad,
                                  PrecioUnitario = x.p.detalle_precio_unitario,
                                  PrecioTotal = x.p.detalle_precio_unitario * x.p.detalle_cantidad,
                                  Imagen = x.prod.pro_imagen // Aquí se agrega la imagen
                              }).ToList()
                          }).FirstOrDefault();

            // Si el pedido no existe, devolver un error
            if (pedido == null)
            {
                return HttpNotFound();
            }

            // Crear el modelo que contiene tanto al usuario como el pedido
            var modelo = new VerUsuarioConPedidos
            {
                Usuario = verUsuario,
                Pedidos = new List<VerPedidoModel> { pedido } // Solo un pedido
            };

            return View(modelo); // Pasar el objeto 'modelo' a la vista
        }


        [HttpPost]
        public ActionResult Registrar(tbl_usuario uvm, HttpPostedFileBase imgfile)
        {
            string path;

            // Si no se sube imagen, asignar imagen por defecto
            if (imgfile == null || imgfile.ContentLength == 0)
            {
                path = "~/Content/Images/PerfilFoto.png"; // Ruta de la imagen por defecto
            }
            else
            {
                path = subirArchivo(imgfile); // Método existente para subir archivos
                if (path.Equals("-1"))
                {
                    ViewBag.error = "No se pudo subir la imagen....";
                    return View(); // Retornar la vista con error
                }
            }

            // Cifrar la contraseña utilizando BCrypt
            string contraseñaCifrada = BCrypt.Net.BCrypt.HashPassword(uvm.u_clave);

            // Limitar la longitud de la contraseña cifrada a 50 caracteres
            if (contraseñaCifrada.Length > 50)
            {
                contraseñaCifrada = contraseñaCifrada.Substring(0, 50); // Truncar a 50 caracteres
            }

            // Crear el usuario con los datos cifrados
            tbl_usuario u = new tbl_usuario
            {
                u_nombre = uvm.u_nombre,
                u_email = uvm.u_email,
                u_clave = uvm.u_clave, // Contraseña cifrada y truncada
                u_imagen = path,
                u_contacto = uvm.u_contacto,
                u_descrip = uvm.u_descrip
            };

            // Guardar el usuario en la base de datos
            db.tbl_usuario.Add(u);
            db.SaveChanges();

            return RedirectToAction("Login");
        }



        // Acción para mostrar el formulario de edición
        public ActionResult EditarPerfil(int id)
        {
            // Verifica si el usuario está autenticado
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login");
            }
            int userId = Convert.ToInt32(Session["u_id"]);

            var usuario = db.tbl_usuario.Find(id);
            if (usuario == null)
            {
                return HttpNotFound(); // Si no se encuentra el usuario
            }
            return View(usuario);
        }

        [HttpPost]
        public ActionResult EditarPerfil(tbl_usuario uvm, HttpPostedFileBase imgfile)
        {
            var usuario = db.tbl_usuario.Find(uvm.u_id);
            if (usuario != null)
            {
                // Actualizar los datos del usuario
                usuario.u_nombre = uvm.u_nombre;
                usuario.u_email = uvm.u_email;
                usuario.u_clave = uvm.u_clave;
                usuario.u_contacto = uvm.u_contacto;
                usuario.u_descrip = uvm.u_descrip;

                // Si se sube una nueva imagen, actualizarla
                if (imgfile != null && imgfile.ContentLength > 0)
                {
                    string path = subirArchivo(imgfile); // Método para subir la imagen
                    if (path.Equals("-1"))
                    {
                        ViewBag.error = "No se pudo subir la imagen....";
                        return View(uvm);
                    }
                    usuario.u_imagen = path; // Guardar la nueva imagen
                }

                try
                {
                    db.SaveChanges(); // Guardar los cambios en la base de datos
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var validationErrors in e.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            // Muestra el error de validación
                            ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                    return View(uvm); // Regresar con los errores de validación
                }

                return RedirectToAction("MiPerfil", new { id = usuario.u_id });
            }

            return View();
        }



        public ActionResult EliminarImagen(int id)
        {
            var usuario = db.tbl_usuario.Find(id);
            if (usuario != null)
            {
                // Cambiar la imagen a la imagen predeterminada
                usuario.u_imagen = "~/Content/Images/PerfilFoto.png"; // Ruta de la imagen predeterminada
                db.SaveChanges();
            }

            return RedirectToAction("EditarPerfil", new { id = usuario.u_id });
        }




        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Login(tbl_usuario avm)
        {
            tbl_usuario ad = db.tbl_usuario.Where(x => x.u_email == avm.u_email && x.u_clave == avm.u_clave).SingleOrDefault();
            if (ad != null)
            {

                Session["u_id"] = ad.u_id.ToString();
                return RedirectToAction("Index");

            }
            else
            {
                ViewBag.error = "Usuario o contraseña incorrecto";

            }

            return View();
        }


        [HttpGet]
        public ActionResult CreatePublic()
        {
            // Verifica si el usuario está autenticado
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login");
            }

            int userId = Convert.ToInt32(Session["u_id"]);

            List<tbl_categoria> li = db.tbl_categoria.ToList();
            ViewBag.categorylist = new SelectList(li, "cat_id", "cat_nombre");
            return View();
        }

        [HttpPost]
        public ActionResult CreatePublic(tbl_producto pvm, HttpPostedFileBase imgfile)
        {   
            List<tbl_categoria> li = db.tbl_categoria.ToList();
            ViewBag.categorylist = new SelectList(li, "cat_id", "cat_nombre");


            string path = subirArchivo(imgfile);
            if (path.Equals("-1"))
            {
                ViewBag.error = "La imagen no se pudo subir....";
            }
            else
            {
                tbl_producto p = new tbl_producto();
                p.pro_nombre = pvm.pro_nombre;
                p.pro_precio = pvm.pro_precio;
                p.pro_imagen = path;
                p.pro_fk_cat = pvm.pro_fk_cat;
                p.pro_descrip = pvm.pro_descrip;
                p.pro_fk_user = Convert.ToInt32(Session["u_id"].ToString());
                db.tbl_producto.Add(p);
                db.SaveChanges();
                Response.Redirect("Index");

            }

            return View();
        }


        public ActionResult Publicaciones(int? id, int? page)
        {
            int pagesize = 6, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;

            // Si 'id' es nulo, lista todos los productos, de lo contrario filtra por categoría.
            var list = id.HasValue
                ? db.tbl_producto.Where(x => x.pro_fk_cat == id).OrderByDescending(x => x.pro_id).ToList()
                : db.tbl_producto.OrderByDescending(x => x.pro_id).ToList();

            IPagedList<tbl_producto> stu = list.ToPagedList(pageindex, pagesize);

            return View(stu);
        }


        [HttpPost]
        public ActionResult Publicaciones(int? id, int? page, string buscar)
        {
            int pagesize = 6, pageindex = 1;
            pageindex = page.HasValue ? Convert.ToInt32(page) : 1;

            // Si 'buscar' tiene valor, busca productos por nombre.
            var list = !string.IsNullOrWhiteSpace(buscar)
                ? db.tbl_producto.Where(x => x.pro_nombre.Contains(buscar)).OrderByDescending(x => x.pro_id).ToList()
                : db.tbl_producto.Where(x => x.pro_fk_cat == id || !id.HasValue).OrderByDescending(x => x.pro_id).ToList();

            IPagedList<tbl_producto> stu = list.ToPagedList(pageindex, pagesize);

            return View(stu);
        }




        public ActionResult VerPublicacion(int? id)
        {
            VerPublicacion ad = new VerPublicacion();
            tbl_producto p = db.tbl_producto.Where(x => x.pro_id == id).SingleOrDefault();
            ad.pro_id = p.pro_id;
            ad.pro_nombre = p.pro_nombre;
            ad.pro_imagen = p.pro_imagen;
            ad.pro_precio = p.pro_precio;
            ad.pro_descrip = p.pro_descrip;
            tbl_categoria cat = db.tbl_categoria.Where(x => x.cat_id == p.pro_fk_cat).SingleOrDefault();
            ad.cat_nombre = cat.cat_nombre;
            tbl_usuario u = db.tbl_usuario.Where(x => x.u_id == p.pro_fk_user).SingleOrDefault();
            ad.u_nombre = u.u_nombre;
            ad.u_imagen = u.u_imagen;
            ad.u_contacto = u.u_contacto;
            ad.pro_fk_user = u.u_id;


            return View(ad);
        }


        public ActionResult ModificarPublicacion(int? id)
        {
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            tbl_producto p = db.tbl_producto.FirstOrDefault(x => x.pro_id == id);
            if (p == null)
            {
                return HttpNotFound();
            }

            VerPublicacion ad = new VerPublicacion
            {
                pro_id = p.pro_id,
                pro_nombre = p.pro_nombre,
                pro_imagen = p.pro_imagen,
                pro_precio = p.pro_precio,
                pro_descrip = p.pro_descrip,
                cat_nombre = db.tbl_categoria.FirstOrDefault(c => c.cat_id == p.pro_fk_cat)?.cat_nombre,
            };


            return View(ad);
        }

        [HttpPost]
        public ActionResult ModificarPublicacion(tbl_producto pvm)
        {
            
                tbl_producto p = db.tbl_producto.FirstOrDefault(x => x.pro_id == pvm.pro_id);
                if (p == null)
                {
                    return HttpNotFound();
                }

                p.pro_nombre = pvm.pro_nombre;                
                p.pro_precio = pvm.pro_precio;
                p.pro_descrip = pvm.pro_descrip;

                db.SaveChanges();

                return RedirectToAction("VerPublicacion", new { id = pvm.pro_id });
        }

            

        public ActionResult Salir()
        {
            Session.RemoveAll();
            Session.Abandon();

            return RedirectToAction("Index");
        }
        public ActionResult DeletePublic(int id)
        {
            // Buscar el producto en la base de datos
            var producto = db.tbl_producto.SingleOrDefault(p => p.pro_id == id);

            if (producto != null)
            {
                // Eliminar primero los registros relacionados en el carrito
                var carritoItems = db.tbl_carrito.Where(c => c.carrito_fk_producto == id).ToList();
                foreach (var item in carritoItems)
                {
                    db.tbl_carrito.Remove(item);
                }

                // Guardar cambios después de eliminar los registros del carrito
                db.SaveChanges();

                // Ahora eliminar el producto
                db.tbl_producto.Remove(producto);
                db.SaveChanges();

                return RedirectToAction("Index"); // Redirigir a la vista principal
            }

            return HttpNotFound();
        }







        public string subirArchivo(HttpPostedFileBase file)
        {
            Random r = new Random();
            string path = "-1";
            int random = r.Next();
            if (file != null && file.ContentLength > 0)
            {
                string extension = Path.GetExtension(file.FileName);
                if (extension.ToLower().Equals(".jpg") || extension.ToLower().Equals(".jpeg") || extension.ToLower().Equals(".png"))
                {
                    try
                    {

                        path = Path.Combine(Server.MapPath("~/Content/upload"), random + Path.GetFileName(file.FileName));
                        file.SaveAs(path);
                        path = "~/Content/upload/" + random + Path.GetFileName(file.FileName);

                        //    ViewBag.Message = "File uploaded successfully";
                    }
                    catch (Exception ex)
                    {
                        path = "-1";
                    }
                }
                else
                {
                    Response.Write("<script>alert('Only jpg ,jpeg or png formats are acceptable....'); </script>");
                }
            }

            else
            {
                Response.Write("<script>alert('Please select a file'); </script>");
                path = "-1";
            }



            return path;
        }

    }
}