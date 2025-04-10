# API de Integración Dragon Ball

## Índice
- [Introducción](#introducción)
- [Prerrequisitos](#prerrequisitos)
- [Obtención del Código](#obtención-del-código)
- [Configuración](#configuración)
  - [Base de Datos (MySQL)](#base-de-datos-mysql)
  - [Configuración JWT](#configuración-jwt)
- [Creación de la Base de Datos (Migraciones)](#creación-de-la-base-de-datos-migraciones)
- [Compilación y Ejecución](#compilación-y-ejecución)
- [Documentación Interactiva (Swagger)](#documentación-interactiva-swagger)
- [Endpoints de la API](#endpoints-de-la-api)
  - [Autenticación](#autenticación)
  - [Sincronización de Datos](#sincronización-de-datos)
  - [Operaciones con Personajes](#operaciones-con-personajes)
  - [Operaciones con Transformaciones](#operaciones-con-transformaciones)
  - [Gestión de Datos](#gestión-de-datos)
- [Uso de JWT](#uso-de-jwt)
- [Consideraciones Adicionales](#consideraciones-adicionales)

## 1. Introducción
Esta API RESTful, desarrollada en ASP.NET Core (.NET 6+), consume la API externa de Dragon Ball para obtener datos de personajes y transformaciones. Solo se almacenan en MySQL los personajes de raza "Saiyan" y, de estos, las transformaciones si el personaje es "Z Fighter". Además, expone endpoints protegidos mediante JWT.

## 2. Prerrequisitos
- SDK de .NET (6.0 o superior): [Descarga aquí](https://dotnet.microsoft.com/download).
- Servidor MySQL (v5.7+): Local o remoto.
- Git para clonar el repositorio.
- *(Opcional)* Cliente de base de datos (MySQL Workbench, DBeaver, etc.).
- *(Opcional)* Herramienta EF Core: `dotnet tool install --global dotnet-ef`.

## 3. Obtención del Código
Clona el repositorio:
```bash
git clone <URL_DEL_REPOSITORIO>
cd <NOMBRE_DEL_DIRECTORIO_DEL_PROYECTO>
```
Reemplaza los marcadores por los valores correspondientes.

## 4. Configuración
La mayor parte de la configuración se encuentra en `appsettings.json`.

### 4.1 Base de Datos (MySQL)
- Crea la base de datos (por ejemplo, `dragonball_db`) y un usuario con permisos.
- Actualiza la cadena de conexión:
```json
"ConnectionStrings": {
  "DefaultConnection": "server=TU_SERVIDOR;port=3306;database=dragonball_db;user=TU_USUARIO;password=TU_CONTRASEÑA;"
}
```

### 4.2 Configuración JWT
Actualiza la sección JWT:
```json
"Jwt": {
  "Key": "ESTA_ES_UNA_CLAVE_MUY_SECRETA_CAMBIALA_12345",
  "Issuer": "https://localhost:5121",
  "Audience": "https://localhost:5121",
  "Expiration": "60"
}
```
*Asegúrate de usar una clave fuerte en producción.*

## 5. Creación de la Base de Datos (Migraciones)
Desde el directorio raíz del proyecto, ejecuta:
```bash
dotnet ef database update
```
Esto crea las tablas `Characters` y `Transformations` según el modelo definido.

## 6. Compilación y Ejecución
Ejecuta:
```bash
dotnet run
```
La salida indicará la URL base (ej., `http://localhost:5123`).

## 7. Documentación Interactiva (Swagger)
Con la API en ejecución, ve a:
```
http://localhost:<PUERTO>/swagger
```
Swagger UI permite:
- Visualizar los endpoints disponibles.
- Probar peticiones (incluyendo la autorización JWT a través del botón "Authorize").

## 8. Endpoints de la API

### Autenticación
- **Endpoint:** POST `/api/auth/login`  
- **Descripción:** Permite obtener el token JWT.  
- **Credenciales de Ejemplo:**  
  Request Body:
  ```json
  {
    "username": "testuser",
    "password": "Pa$$w0rd"
  }
  ```
  *Nota: La contraseña real según el código es "Pa$$w0rd", no "password123".*  
- **Respuesta Exitosa (200 OK):**
  ```json
  {
    "token": "TOKEN_JWT_GENERADO"
  }
  ```
- **Errores:** Retorna 401 en caso de credenciales inválidas.

### Sincronización de Datos
- **Endpoint:** POST `/api/characters/sync`  
- **Descripción:** Sincroniza personajes y transforma a partir de la API externa.  
- **Reglas de Negocio:**  
  - Retorna 409 si ya existen registros en las tablas.
  - Solo se guardan personajes con `Race` igual a "Saiyan".
  - Si el personaje es "Z Fighter", se guardan también sus transformaciones.  
- **Respuesta:** Mensaje con la cantidad de personajes y transformaciones sincronizadas.  

### Operaciones con Personajes
- **Endpoint:** GET `/api/characters`  
  Recupera todos los personajes con sus transformaciones.
  
- **Endpoint:** GET `/api/characters/{id}`  
  Recupera un personaje por su ID.

- **Endpoint:** GET `/api/characters/byName/{name}`  
  Busca personajes cuyo nombre contenga el valor proporcionado.  
  - *Ejemplo de error:* Si el parámetro está vacío, retorna 400.

- **Endpoint:** GET `/api/characters/byAffiliation/{affiliation}`  
  Busca personajes con la afiliación exacta (por ejemplo, "Z Fighter").  
  - *Ejemplo de error:* Parámetro vacío retorna 400.

### Operaciones con Transformaciones
- **Endpoint:** GET `/api/transformations`  
  Recupera todas las transformaciones, incluyendo el `characterId`.
  
- **Endpoint:** GET `/api/transformations/{id}`  
  Recupera la transformación con el ID especificado.

### Gestión de Datos
- **Endpoint:** DELETE `/api/datamanagement/clear`  
  Permite borrar de forma manual todos los datos de personajes y transformaciones en la base de datos.  
  - *Uso:* Ideal para reiniciar el estado antes de una nueva sincronización.
  - **Respuesta:** Mensaje de confirmación o error en caso de fallo.

## 9. Uso de JWT
Para acceder a los endpoints protegidos:
1. Realiza una petición POST a `/api/auth/login` con las credenciales.
2. Copia el token recibido.
3. Incluye en cada petición protegida la cabecera:
```
Authorization: Bearer <TOKEN_JWT>
```
En Swagger, haz clic en "Authorize" e ingresa el token (precedido de "Bearer ").

## 10. Consideraciones Adicionales
- **Logs:** La API registra información y advertencias en la consola para detectar problemas, especialmente durante la sincronización.
- **Manejo de Errores:** Se recomienda implementar middleware global para gestionar excepciones.
- **Seguridad:** Nunca expongas claves ni credenciales; utiliza User Secrets o variables de entorno.
- **Reinicio de Datos:** Antes de una nueva sincronización, elimina los datos existentes usando el endpoint DELETE `/api/datamanagement/clear` o mediante una actualización de la lógica de negocio.
