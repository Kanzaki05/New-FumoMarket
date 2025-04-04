using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using Market.Models;
using PagedList;


namespace Marketplace.Controllers
{
    public class AdminController : Controller
    {
        dbMarketplaceEntities db = new dbMarketplaceEntities();
        // GET: Admin
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Login(tbl_admin avm)
        {
            tbl_admin ad = db.tbl_admin.Where(x => x.ad_usuario == avm.ad_usuario && x.ad_clave == avm.ad_clave).SingleOrDefault();
            if (ad != null)
            {

                Session["ad_id"] = ad.ad_id.ToString();
                return RedirectToAction("Categoria");

            }
            else
            {
                ViewBag.error = "Usuario o contraseña incorrecto";

            }

            return View();
        }


        public ActionResult Create()
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }


        [HttpPost]
        public ActionResult Create(tbl_categoria cvm, HttpPostedFileBase imagen)
        {
            if (imagen != null && imagen.ContentLength > 0)
            {
                try
                {
                    string path = subirArchivo(imagen);
                    if (path.Equals("-1"))
                    {
                        ViewBag.error = "La imagen no se pudo subir....";
                    }
                    else
                    {
                        tbl_categoria cat = new tbl_categoria();
                        cat.cat_nombre = cvm.cat_nombre;
                        cat.cat_imagen = path;
                        cat.cat_estado = 1;
                        cat.cat_fk_ad = Convert.ToInt32(Session["ad_id"].ToString());
                        db.tbl_categoria.Add(cat);
                        db.SaveChanges();
                        return RedirectToAction("Categoria");
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.error = "Ocurrió un error al subir la imagen: " + ex.Message;
                }
            }
            else
            {
                ViewBag.error = "Por favor seleccione un archivo para subir.";
            }

            return View();
        }




        public ActionResult Categoria(int? page)
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("Login");
            }
            else { 
                int pagesize = 6, pageindex = 1;
                pageindex = page.HasValue ? Convert.ToInt32(page) : 1;
                var list = db.tbl_categoria.Where(x => x.cat_estado == 1).OrderByDescending(x => x.cat_id).ToList();
                IPagedList<tbl_categoria> stu = list.ToPagedList(pageindex, pagesize);

                return View(stu);
            }
        }

        public ActionResult VerUsuario(int? id)
        {
            VerUsuario vu = new VerUsuario();
            tbl_usuario us = db.tbl_usuario.Where(x => x.u_id == id).SingleOrDefault();
            List<tbl_producto> productosDelUsuario = db.tbl_producto.Where(p => p.pro_fk_user == id).ToList();

            {
                vu.u_id = us.u_id;
                vu.u_nombre = us.u_nombre;
                vu.u_email = us.u_email;
                vu.u_contacto = us.u_contacto;
                vu.u_imagen = us.u_imagen;
                vu.u_descrip = us.u_descrip;
                vu.Productos = productosDelUsuario.Select(p => new VerPublicacion
                {
                    pro_id = p.pro_id,
                    pro_nombre = p.pro_nombre,
                    pro_imagen = p.pro_imagen,
                    pro_descrip = p.pro_descrip,
                    pro_precio = p.pro_precio,
                    pro_fk_cat = p.pro_fk_cat,
                    pro_fk_user = p.pro_fk_user,
                    cat_nombre = p.tbl_categoria.cat_nombre,
                    u_nombre = us.u_nombre,
                    u_imagen = us.u_imagen,
                    u_contacto = us.u_contacto
                }).ToList();
            };
            
            return View(vu);
        }

        public ActionResult Usuarios(int? page)
        {
            if (Session["ad_id"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                int pageSize = 6, pageIndex = 1;
                pageIndex = page.HasValue ? Convert.ToInt32(page) : 1;

                var list = db.tbl_usuario.OrderBy(x => x.u_id).ToList();
                IPagedList<tbl_usuario> usuarios = list.ToPagedList(pageIndex, pageSize);

                return View(usuarios);
            }
        }

        public ActionResult DeleteUsuario(int? id)
        {
            tbl_usuario usuario = db.tbl_usuario.Include("tbl_producto").SingleOrDefault(x => x.u_id == id);

            if (usuario == null)
            {
                return HttpNotFound();
            }

            foreach (var producto in usuario.tbl_producto.ToList())
            {
                db.tbl_producto.Remove(producto);
            }

            db.tbl_usuario.Remove(usuario);

            db.SaveChanges();

            return RedirectToAction("Usuarios");
        }

        public ActionResult DeleteProducto(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Buscar el producto por ID, incluyendo la relación con el usuario si es necesario
            tbl_producto producto = db.tbl_producto.Include("tbl_usuario").SingleOrDefault(p => p.pro_id == id);

            if (producto == null)
            {
                return HttpNotFound();
            }

            // Eliminar el producto
            db.tbl_producto.Remove(producto);

            // Guardar los cambios en la base de datos
            db.SaveChanges();

            // Recargar la página desde la que se realizó la solicitud
            return Redirect(Request.UrlReferrer.ToString());
        }


        public ActionResult Salir()
        {
            Session.RemoveAll();
            Session.Abandon();

            return RedirectToAction("Login");
        }
        public ActionResult DeleteCategoria(int? id)
        {            
            tbl_categoria categoria = db.tbl_categoria.Include("tbl_producto").SingleOrDefault(x => x.cat_id == id);
                        
            if (categoria == null)
            {
                return HttpNotFound();
            }
                      
            foreach (var producto in categoria.tbl_producto.ToList())
            {
                db.tbl_producto.Remove(producto);
            }
                        
            db.tbl_categoria.Remove(categoria);
                       
            db.SaveChanges();

            return RedirectToAction("Categoria");
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
                    Response.Write("<script>alert('Solo los formatos jpg ,jpeg or png son aceptados....'); </script>");
                }
            }

            else
            {
                Response.Write("<script>alert('Por favor seleccionar un archivo'); </script>");
                path = "-1";
            }



            return path;
        }








    }
}
