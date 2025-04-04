Create Database dbMarketplace

Use dbMarketplace


Create table tbl_admin
(
ad_id int identity primary key,
ad_usuario nvarchar(50) not null unique,
ad_clave nvarchar(50) not null,
)

insert into tbl_admin
values('admin','admin123')
insert into tbl_admin
values('root','admin123')
insert into tbl_admin
values('test','admin123')

select * from tbl_admin


Create table tbl_categoria
(
cat_id int identity primary key,
cat_nombre nvarchar(50) not null unique,
cat_imagen nvarchar(max) not null,
cat_fk_ad int foreign key references tbl_admin(ad_id),
cat_estado int not null,
)

CREATE TABLE tbl_direccion 
(
    direc_id INT IDENTITY(1,1) PRIMARY KEY, 
    direc_pais NVARCHAR(100) NOT NULL,      
    direc_provincia NVARCHAR(100) NOT NULL, 
    direc_distrito NVARCHAR(100) NOT NULL,  
    direc_postal NVARCHAR(20) NOT NULL,     
    direc_linea1 NVARCHAR(255) NOT NULL,    
    direc_linea2 NVARCHAR(255) NULL         
);

Create table tbl_usuario
(
u_id int identity primary key,
u_nombre nvarchar(50) not null,
u_email nvarchar(50) not null unique,
u_clave nvarchar(50) not null,
u_imagen nvarchar(max) not null,
u_contacto nvarchar(50) not null unique,
u_descrip nvarchar(300) null,
);

CREATE TABLE tbl_usuario_direccion 
(
    ud_id INT IDENTITY(1,1) PRIMARY KEY,   -- Clave primaria
    ud_u_id INT FOREIGN KEY REFERENCES tbl_usuario(u_id),  -- Clave foránea a usuario
    ud_direc_id INT FOREIGN KEY REFERENCES tbl_direccion(direc_id), -- Clave foránea a direccion
    ud_activa BIT DEFAULT 1                -- Campo para marcar la dirección activa o no
);

Create table tbl_producto
(
pro_id int identity primary key,
pro_nombre nvarchar(50) not null,
pro_imagen nvarchar(max) not null,
pro_descrip nvarchar(max) not null,
pro_precio int,
pro_fk_user int foreign key references tbl_usuario(u_id),
pro_fk_cat int foreign key references tbl_categoria(cat_id),
);

CREATE TABLE tbl_carrito
(
    carrito_id INT IDENTITY PRIMARY KEY,
    carrito_fk_usuario INT FOREIGN KEY REFERENCES tbl_usuario(u_id),
    carrito_fk_producto INT FOREIGN KEY REFERENCES tbl_producto(pro_id),
    carrito_cantidad INT NOT NULL DEFAULT 1,
    carrito_fecha DATETIME DEFAULT GETDATE()
);

CREATE TABLE tbl_compra
(
    compra_id INT IDENTITY(1,1) PRIMARY KEY,               -- Clave primaria
    compra_fk_usuario INT NOT NULL FOREIGN KEY REFERENCES tbl_usuario(u_id),  -- Usuario que realiza la compra
    compra_fk_direccion INT NOT NULL FOREIGN KEY REFERENCES tbl_usuario_direccion(ud_id), -- Dirección de envío
    compra_fecha DATETIME DEFAULT GETDATE(),              -- Fecha de la compra
    compra_total DECIMAL(10, 2) NOT NULL,                 -- Total de la compra
    compra_metodo_pago NVARCHAR(50) NOT NULL,             -- Método de pago (e.g., 'Tarjeta', 'Efectivo', 'PayPal')
    compra_observaciones NVARCHAR(MAX) NULL              -- Campo opcional para observaciones
);

CREATE TABLE tbl_pago
(
    pago_id INT IDENTITY(1,1) PRIMARY KEY,               -- Identificador único del pago
    pago_tipo NVARCHAR(50) NOT NULL,                    -- Tipo de pago (e.g., Tarjeta de Crédito, Débito)
    pago_tarjeta NVARCHAR(20) NOT NULL,                 -- Número de tarjeta (en formato seguro, sin almacenar completo)
    pago_caducidad NVARCHAR(5) NOT NULL,                -- Fecha de caducidad de la tarjeta (MM/YY)
    pago_cvv NVARCHAR(4) NOT NULL,                      -- CVV de la tarjeta (almacenar con seguridad)
    pago_fk_compra INT NOT NULL FOREIGN KEY REFERENCES tbl_compra(compra_id), -- Relación con la compra
    pago_fecha DATETIME DEFAULT GETDATE(),              -- Fecha del pago
);

CREATE TABLE tbl_detalle_compra (
    detalle_id INT IDENTITY(1,1) PRIMARY KEY, -- Identificador único
    detalle_fk_compra INT NOT NULL, -- Relación con la tabla de compra
    detalle_fk_producto INT NOT NULL, -- Relación con la tabla de producto
    detalle_cantidad INT NOT NULL, -- Cantidad de producto comprado
    detalle_precio_unitario DECIMAL(10, 2) NOT NULL, -- Precio unitario del producto

    -- Claves foráneas
    CONSTRAINT fk_detalle_compra FOREIGN KEY (detalle_fk_compra)
        REFERENCES tbl_compra (compra_id)
        ON DELETE CASCADE ON UPDATE CASCADE,

    CONSTRAINT fk_detalle_producto FOREIGN KEY (detalle_fk_producto)
        REFERENCES tbl_producto (pro_id)
        ON DELETE CASCADE ON UPDATE CASCADE
);


DELETE FROM tbl_detalle_compra;
DELETE FROM tbl_pago;
DELETE FROM tbl_compra;



select * from tbl_usuario
select * from tbl_detalle_compra
select * from tbl_compra
select * from tbl_pago
select * from tbl_categoria
select * from tbl_producto
select * from tbl_carrito


