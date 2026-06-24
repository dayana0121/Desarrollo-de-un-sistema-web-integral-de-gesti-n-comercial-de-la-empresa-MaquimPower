# 📦 E1 - Sistema Integral de Gestión Empresarial

Sistema completo de gestión empresarial desarrollado en **ASP.NET Core MVC** con **.NET 8** y **SQL Server**. Incluye módulos para ventas, compras, inventario, caja, seguridad y más.

---

## 🎯 Descripción General

E1 es una aplicación web empresarial que facilita: 
- Gestión de ventas y cotizaciones
- Control de compras y proveedores
- Administración de inventario y almacenes
- Gestión de caja y movimientos
- Sistema de seguridad con roles y permisos
- Administración de clientes, productos y terceros

---

## 📋 Requisitos Previos

Antes de ejecutar el proyecto, asegúrate de tener instalado:

- **Visual Studio 2019** o superior (preferible **Visual Studio 2026**)
- **.NET 8 SDK** o superior
- **SQL Server** 2019 o superior (Enterprise, Express o Developer Edition)
- **SQL Server Management Studio** (SSMS) - Recomendado para gestionar la BD

---

## 🗂️ Estructura del Proyecto

```
E1/
├── Controllers/              # Controladores MVC (Lógica de negocio)
│   ├── CajaController.cs
│   ├── ComprasController.cs
│   ├── ConfiguracionController.cs
│   ├── DashboardController.cs
│   ├── HomeController.cs
│   ├── InventarioController.cs
│   ├── LoginController.cs
│   ├── ProductosController.cs
│   ├── SeguridadController.cs
│   ├── TercerosController.cs
│   └── VentasController.cs
│
├── Data/                     # Contexto y configuración de BD
│   └── AppDbContext.cs       # DbContext principal (EF Core)
│
├── Models/                   # Modelos de entidades
│   ├── Usuario.cs
│   ├── Rol.cs
│   ├── Permiso.cs
│   ├── Producto.cs
│   ├── Venta.cs
│   ├── Compra.cs
│   ├── Inventario.cs
│   └── [+20 modelos más]
│
├── Views/                    # Vistas Razor (.cshtml)
│   ├── Home/
│   ├── Dashboard/
│   ├── Ventas/
│   ├── Compras/
│   ├── Inventario/
│   ├── Caja/
│   ├── Seguridad/
│   ├── Terceros/
│   ├── Productos/
│   ├── Configuracion/
│   └── Shared/
│
├── Helpers/                  # Utilitarios
│   ├── AccessHelper.cs       # Control de acceso
│   └── SessionHelper.cs      # Gestión de sesiones
│
├── wwwroot/                  # Recursos estáticos
│   ├── css/
│   ├── js/
│   ├── lib/                  # Librerías (Bootstrap, jQuery, DataTables)
│   └── favicon.ico
│
├── Properties/
│   └── launchSettings.json   # Configuración de ejecución
│
├── appsettings.json          # Configuración general
├── appsettings.Development.json
├── Program.cs                # Configuración de servicios y middleware
└── E1.csproj                 # Definición del proyecto
```

---

## 🔧 Configuración de la Base de Datos

### 1️⃣ **Verificar la conexión SQL Server**

La cadena de conexión está en `appsettings.json`:

```json
"ConnectionStrings": {
  "MaquimPowerDB": "Server=DESKTOP-KRICA6B;Database=MaquimPowerDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 2️⃣ **Cambiar servidor o parámetros de conexión**

Si necesitas modificar la conexión a SQL Server (por ejemplo, usar otro servidor):

**Opción A: Editar `appsettings.json`**

```json
"ConnectionStrings": {
  "MaquimPowerDB": "Server=TU_SERVIDOR;Database=MaquimPowerDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

**Parámetros comunes:**
- `Server`: Nombre del servidor SQL Server (ej: `DESKTOP-KRICA6B`, `.`, `(local)`, `servidor.dominio.com`)
- `Database`: Nombre de la base de datos (por defecto: `MaquimPowerDB`)
- `Trusted_Connection=True`: Usa autenticación de Windows
- `User Id` / `Password`: Para autenticación SQL Server en lugar de Windows

**Ejemplo con autenticación SQL Server:**
```json
"ConnectionStrings": {
  "MaquimPowerDB": "Server=DESKTOP-KRICA6B;Database=MaquimPowerDB;User Id=sa;Password=tuPassword;TrustServerCertificate=True;"
}
```

### 3️⃣ **Crear/Actualizar la base de datos**

#### **Opción A: Usando Package Manager Console (Visual Studio)**

1. Abre **Tools > NuGet Package Manager > Package Manager Console**
2. Ejecuta:
```powershell
Update-Database
```

#### **Opción B: Usando terminal PowerShell**

1. Abre PowerShell en la carpeta del proyecto
2. Ejecuta:
```powershell
dotnet ef database update
```

#### **Opción C: Crear manualmente con SQL Server Management Studio**

1. Abre **SQL Server Management Studio (SSMS)**
2. Crea una base de datos llamada `MaquimPowerDB`
3. El proyecto ejecutará las migraciones automáticamente

---

## 🚀 Ejecución del Proyecto

### **Desde Visual Studio**

1. Abre el archivo de solución: `E1.sln`
2. Presiona **F5** para ejecutar en modo Debug
   - O **Ctrl+F5** para ejecutar sin Debug
3. Se abrirá el navegador en `https://localhost:puerto`

### **Desde PowerShell / Command Line**

```powershell
cd "C:\ruta\a\E1"
dotnet run
```

La aplicación estará disponible en: `https://localhost:5001` (o el puerto configurado en `launchSettings.json`)

---

## 🔐 Autenticación y Seguridad

El proyecto cuenta con:

- **Sistema de login** (`LoginController`)
- **Gestión de usuarios** (`SeguridadController`)
- **Roles y permisos** (Control basado en roles)
- **Sesiones** (Duración: 8 horas)
- **AccessHelper**: Valida permisos antes de acceder a acciones

### Flujo de seguridad:
1. El usuario inicia sesión en `/Login`
2. Se valida contra la tabla `Usuarios`
3. Se obtienen los roles y permisos del usuario
4. Los controladores verifican permiso antes de ejecutar acciones

---

## 📊 Módulos Principales

### 1. **Dashboard** (`DashboardController`)
- Resumen de ventas, compras e inventario
- Indicadores clave del negocio

### 2. **Ventas** (`VentasController`)
- Crear y gestionar ventas
- Cotizaciones y seguimiento de estados
- Historial de ventas

### 3. **Compras** (`ComprasController`)
- Registrar compras a proveedores
- Detalles de compra
- Control de recepción

### 4. **Inventario** (`InventarioController`)
- Gestión de almacenes
- Movimientos de stock
- Traslados entre almacenes

### 5. **Caja** (`CajaController`)
- Registrar movimientos de caja
- Seguimiento de ingresos y egresos
- Arqueo de caja

### 6. **Seguridad** (`SeguridadController`)
- Gestión de usuarios
- Asignación de roles
- Permisos por rol

### 7. **Terceros** (`TercerosController`)
- Gestión de clientes
- Gestión de proveedores
- Datos de contacto

### 8. **Productos** (`ProductosController`)
- Catálogo de productos
- Categorías y marcas
- Precios y características

### 9. **Configuración** (`ConfiguracionController`)
- Parámetros de la empresa
- Datos generales

---

## 💾 Base de Datos - Tablas Principales

El contexto `AppDbContext` gestiona las siguientes tablas:

**Configuración:**
- `Empresas`

**Seguridad:**
- `Usuarios`, `Roles`, `Permisos`, `AccesosPorRol`

**Terceros:**
- `Clientes`, `Proveedores`

**Productos:**
- `Categorias`, `Marcas`, `Productos`

**Compras:**
- `Compras`, `DetalleCompras`

**Inventario:**
- `Almacenes`, `MovimientosStock`, `Traslados`, `DetalleTraslados`

**Caja:**
- `Cajas`, `MovimientosCaja`

**Ventas:**
- `EstadosVenta`, `Cotizaciones`, `DetalleCotizaciones`, `Ventas`, `DetalleVentas`

---

## 📦 Dependencias Principales

El proyecto usa:

- **Entity Framework Core 8.0** - ORM para acceso a datos
- **Microsoft.Data.SqlClient 7.0.1** - Conexión con SQL Server
- **Dapper 2.1.79** - Consultas SQL adicionales
- **Bootstrap** - Framework CSS
- **jQuery** - Manipulación del DOM
- **DataTables** - Tablas interactivas

---

## ⚙️ Configuración del Proyecto (Program.cs)

El archivo `Program.cs` configura:

```csharp
// Servicios MVC
builder.Services.AddControllersWithViews();

// Entity Framework + SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("MaquimPowerDB")));

// Sesiones (duración: 8 horas)
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromHours(8);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

// Contexto HTTP
builder.Services.AddHttpContextAccessor();
```

---

## 🐛 Solución de Problemas

### ❌ Error: "Cannot connect to server"
- Verifica que SQL Server esté funcionando
- Comprrueba el nombre del servidor en `appsettings.json`
- Usa `(local)` o `.` si está en tu máquina local

### ❌ Error: "Database doesn't exist"
- Ejecuta `Update-Database` desde Package Manager Console
- O crea la BD manualmente en SSMS

### ❌ Error de migraciones
- Asegúrate de tener `Microsoft.EntityFrameworkCore.Tools` instalado
- Ejecuta: `dotnet ef migrations add InitialCreate`

### ❌ Error de puerto en uso
- Modifica el puerto en `launchSettings.json`
- O detiene otros procesos que usan ese puerto

---

## 📝 Notas Importantes

✅ **Este proyecto está diseñado para ejecutarse SOLO en Visual Studio**

✅ **La BD debe estar en SQL Server** (no es compatible con SQLite o PostgreSQL sin cambios)

✅ **La autenticación es con Windows** (por defecto, Trusted_Connection=True)

✅ **Las sesiones se guardan en memoria** (perfectas para devtest, considera persistencia en producción)

✅ **Los roles y permisos se validan en cada petición** a través de `AccessHelper`

---

## 🔍 Recursos Adicionales

- **Entity Framework Core**: https://docs.microsoft.com/ef/core/
- **ASP.NET Core MVC**: https://docs.microsoft.com/aspnet/core/
- **SQL Server Documentation**: https://docs.microsoft.com/sql/

---

## 👨‍💻 Autor

**E1 - Sistema Integral de Gestión**  
**Trabajo realizado en equipo por:**
- Albornoz Nava Julio
- Caigua Burguillos Dylan
- Carrión Espinoza Dayana
- Galiano Acuña Diego
- Morales Moreno Edinson
- Vergara Tafur Kenia
**Estudiantes de la carrera de Desarrollo de Software del quinto semestre - Senati**
Desarrollado con ASP.NET Core 8 y SQL Server

---

**Versión:** 1.0
**2026**
