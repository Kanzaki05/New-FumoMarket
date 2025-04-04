using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Market.Models;

namespace Market.Controllers
{
    public class CarritoController : Controller
    {
        private dbMarketplaceEntities db = new dbMarketplaceEntities();

        // Acción para añadir producto al carrito
        [HttpPost]
        public ActionResult AgregarAlCarrito(int productoId)
        {
            int userId = Convert.ToInt32(Session["u_id"]);
            var carritoItem = db.tbl_carrito.FirstOrDefault(c => c.carrito_fk_producto == productoId && c.carrito_fk_usuario == userId);

            if (carritoItem != null)
            {
                carritoItem.carrito_cantidad += 1;
            }
            else
            {
                tbl_carrito nuevoCarrito = new tbl_carrito
                {
                    carrito_fk_usuario = userId,
                    carrito_fk_producto = productoId,
                    carrito_cantidad = 1,
                    carrito_fecha = DateTime.Now
                };
                db.tbl_carrito.Add(nuevoCarrito);
            }

            db.SaveChanges();
            return Redirect(Request.UrlReferrer.ToString());
        }

        // Acción para mostrar el carrito
        public ActionResult VerCarrito()
        {
            // Verifica si el usuario está autenticado
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login", "Usuario");
            }
            int userId = Convert.ToInt32(Session["u_id"]);
            var carrito = db.tbl_carrito.Where(c => c.carrito_fk_usuario == userId).ToList();
            return View(carrito);
        }

        // Acción para eliminar producto del carrito
        
        public ActionResult EliminarDelCarrito(int carritoId)
        {
            var carritoItem = db.tbl_carrito.Find(carritoId);
            if (carritoItem != null)
            {
                db.tbl_carrito.Remove(carritoItem);
                db.SaveChanges();
            }
            return RedirectToAction("VerCarrito");
        }

        // Acción para incrementar la cantidad de un producto en el carrito
       
        public ActionResult IncrementarCantidad(int carritoId)
        {
            var carritoItem = db.tbl_carrito.Find(carritoId);
            if (carritoItem != null)
            {
                carritoItem.carrito_cantidad += 1;
                db.SaveChanges();
            }
            return RedirectToAction("VerCarrito");
        }

        // Acción para decrementar la cantidad de un producto en el carrito
        
        public ActionResult DecrementarCantidad(int carritoId)
        {
            var carritoItem = db.tbl_carrito.Find(carritoId);
            if (carritoItem != null && carritoItem.carrito_cantidad > 1)
            {
                carritoItem.carrito_cantidad -= 1;
                db.SaveChanges();
            }
            return RedirectToAction("VerCarrito");
        }

        public ActionResult Comprar()
        {
            // Verifica si el usuario está autenticado
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login", "Usuario");
            }
            // Verificar si el carrito tiene productos para proceder a la compra
            int userId = Convert.ToInt32(Session["u_id"]);
            var carrito = db.tbl_carrito.Where(c => c.carrito_fk_usuario == userId).ToList();

            if (carrito == null || !carrito.Any())
            {
                TempData["Mensaje"] = "Tu carrito está vacío, agrega productos antes de continuar.";
                return RedirectToAction("VerCarrito");
            }

            // Lógica adicional si quieres enviar los datos del carrito a la vista
            ViewBag.Total = carrito.Sum(item => item.carrito_cantidad * item.tbl_producto.pro_precio);
            return View("CompraForm"); // Nombre de la vista del formulario
        }

        [HttpPost]
        public ActionResult ProcesarCompra(FormCollection form)
        {
            // Verifica si el usuario está autenticado
            if (Session["u_id"] == null)
            {
                return RedirectToAction("Login", "Usuario");
            }

            int userId = Convert.ToInt32(Session["u_id"]);
            var carrito = db.tbl_carrito.Where(c => c.carrito_fk_usuario == userId).ToList();

            if (carrito == null || !carrito.Any())
            {
                TempData["Mensaje"] = "Tu carrito está vacío, agrega productos antes de continuar.";
                return RedirectToAction("VerCarrito");
            }

            // Recoger datos del formulario de pago
            string tipoPago = form["tipoPago"];
            string numeroTarjeta = form["tarjeta"];
            string caducidad = form["caducidad"];
            string cvv = form["cvv"];

            // Recoger datos de la dirección de envío
            string pais = form["pais"];
            string provincia = form["provincia"];
            string ciudad = form["ciudad"];
            string direccion1 = form["direccion1"];
            string direccion2 = form["direccion2"];
            string codigoPostal = form["codigoPostal"];

            // Validación básica (opcional, puede ser más estricta)
            if (string.IsNullOrWhiteSpace(tipoPago) || string.IsNullOrWhiteSpace(numeroTarjeta) ||
                string.IsNullOrWhiteSpace(caducidad) || string.IsNullOrWhiteSpace(cvv) ||
                string.IsNullOrWhiteSpace(pais) || string.IsNullOrWhiteSpace(direccion1))
            {
                TempData["Error"] = "Por favor, completa todos los campos obligatorios.";
                return RedirectToAction("Comprar");
            }

            // Crear la dirección y agregarla a la tabla tbl_direccion
            tbl_direccion nuevaDireccion = new tbl_direccion
            {
                direc_pais = pais,
                direc_provincia = provincia,
                direc_distrito = ciudad,
                direc_postal = codigoPostal,
                direc_linea1 = direccion1,
                direc_linea2 = direccion2
            };
            db.tbl_direccion.Add(nuevaDireccion);
            db.SaveChanges();  // Guardar la dirección y obtener el ID

            // Asociar la dirección con el usuario en tbl_usuario_direccion
            tbl_usuario_direccion usuarioDireccion = new tbl_usuario_direccion
            {
                ud_u_id = userId,
                ud_direc_id = nuevaDireccion.direc_id,
                ud_activa = true
            };
            db.tbl_usuario_direccion.Add(usuarioDireccion);
            db.SaveChanges();  // Guardar la relación

            // Crear un nuevo registro de compra con el ID de la dirección
            tbl_compra nuevaCompra = new tbl_compra
            {
                compra_fk_usuario = userId,
                compra_fecha = DateTime.Now,
                compra_total = carrito.Sum(item => (decimal)(item.carrito_cantidad) * (decimal)(item.tbl_producto.pro_precio)),
                compra_metodo_pago = tipoPago,
                compra_fk_direccion = nuevaDireccion.direc_id // Aquí se guarda el ID de la dirección
            };

            db.tbl_compra.Add(nuevaCompra);
            db.SaveChanges();

            // Registrar el pago en la tabla tbl_pago
            tbl_pago nuevoPago = new tbl_pago
            {
                pago_tipo = tipoPago,
                pago_tarjeta = numeroTarjeta,  // Puedes enmascarar el número de tarjeta por seguridad
                pago_caducidad = caducidad,
                pago_cvv = cvv,  // Asegúrate de que se maneje de manera segura
                pago_fk_compra = nuevaCompra.compra_id,  // Relacionar el pago con la compra
                pago_fecha = DateTime.Now
            };

            db.tbl_pago.Add(nuevoPago);
            db.SaveChanges();

            // Procesar los productos del carrito y guardarlos en los detalles de la compra
            foreach (var item in carrito)
            {
                tbl_detalle_compra detalle = new tbl_detalle_compra
                {
                    detalle_fk_compra = nuevaCompra.compra_id,
                    detalle_fk_producto = (int)item.carrito_fk_producto,
                    detalle_cantidad = item.carrito_cantidad,  // Usar 0 si es null
                    detalle_precio_unitario = item.tbl_producto.pro_precio ?? 0.0m  // Usar 0.0m si es null
                };

                db.tbl_detalle_compra.Add(detalle);
            }

            // Limpiar el carrito después de completar la compra
            db.tbl_carrito.RemoveRange(carrito);
            db.SaveChanges();

            TempData["Exito"] = "¡Compra realizada con éxito!";
            return RedirectToAction("VerCarrito");
        }

    }
}
