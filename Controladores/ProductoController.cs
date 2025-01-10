using API_TIENDA.Servicios;
using E_Commerce_API.Dto;
using E_Commerce_API.Modelos;
using E_Commerce_API.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace E_Commerce_API.Controladores
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductoController : ControllerBase
    {
        private readonly DbEcommerce _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ICloudinaryService _cloudinaryService;

        public ProductoController(DbEcommerce context, IWebHostEnvironment environment, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _environment = environment;
            _cloudinaryService = cloudinaryService;

        }
        [HttpGet]
        public async Task<IActionResult> GetAllProduct()
        {
            try
            {
                var productos = await _context.Productos.Include(p => p.Categoria).Select(p => new ProductoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    Descripcion = p.Descripcion,
                    ImagenUrl = p.ImagenUrl,
                    Categoria = new CategoriaDto
                    {
                        Id = p.Categoria.Id,
                        Nombre = p.Categoria.Nombre,
                        Descripcion = p.Categoria.Descripcion,
                        ImagenUrl = p.Categoria.ImagenUrl,
                    }
                }).ToListAsync();

                return Ok(new { message = "Todos Los Productos : ", productos }); // 200 OK
            }
            catch (Exception ex)
            {
                // Manejo de error inesperado
                return StatusCode(500, new { message = "Error al obtener los productos.", details = ex.Message }); // 500 Internal Server Error
            }
        }

        [HttpGet]
        [Route("BuscarProducto/{id}")]
        public async Task<IActionResult> GetSearchProduct(int id)
        {
            try
            {
                // Obtener el rol desde el token (ya está validado por [Authorize])
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                // Validar si el usuario no es administrador
                if (role == "Cliente")
                {
                    return StatusCode(403, new { message = "Acceso denegado. Este recurso solo está disponible para administradores y Empleados." });
                }
                var producto = await _context.Productos
                    .Include(p => p.Categoria) // Incluir la relación con la categoría
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (producto == null)
                {
                    return NotFound(new { message = "Producto no encontrado." }); // 404 Not Found
                }

                var productoDto = new ProductoDto
                {
                    Id = producto.Id,
                    Nombre = producto.Nombre,
                    Precio = producto.Precio,
                    Descripcion = producto.Descripcion,
                    ImagenUrl = producto.ImagenUrl,
                    Categoria = new CategoriaDto
                    {
                        Id = producto.Categoria.Id,
                        Nombre = producto.Categoria.Nombre
                    }
                };

                return Ok(new { message = "El Producto es:", productoDto }); // 200 OK
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el producto.", details = ex.Message }); // 500 Internal Server Error
            }
        }


        [HttpGet]
        [Route("BuscarProductoNombre/{nombre}")]
        public async Task<IActionResult> GetSearchProductByName(string nombre)
        {
            try
            {
                // Buscar el producto por nombre (ignorando mayúsculas/minúsculas)
                var producto = await _context.Productos
                    .Include(p => p.Categoria) // Incluir la relación con la categoría
                    .FirstOrDefaultAsync(p => p.Nombre.ToLower() == nombre.ToLower());

                if (producto == null)
                {
                    return NotFound(new { message = "Producto no encontrado." }); // 404 Not Found
                }

                // Mapear al DTO
                var productoDto = new ProductoDto
                {
                    Id = producto.Id,
                    Nombre = producto.Nombre,
                    Precio = producto.Precio,
                    Descripcion = producto.Descripcion,
                    ImagenUrl = producto.ImagenUrl,
                    Categoria = new CategoriaDto
                    {
                        Id = producto.Categoria.Id,
                        Nombre = producto.Categoria.Nombre,
                        Descripcion = producto.Categoria.Descripcion,
                        ImagenUrl = producto.Categoria.ImagenUrl,
                    }
                };

                return Ok(new { message = "El Producto encontrado es:", productoDto }); // 200 OK
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener el producto.", details = ex.Message }); // 500 Internal Server Error
            }
        }

        [HttpGet]
        [Route("PaginacionProducto")]
        public async Task<IActionResult> GetPaginatedProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var totalProductos = await _context.Productos.CountAsync();

                var productos = await _context.Productos
                    .Include(p => p.Categoria) // Incluir la relación con la categoría
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(producto => new ProductoDto
                    {
                        Id = producto.Id,
                        Nombre = producto.Nombre,
                        Precio = producto.Precio,
                        ImagenUrl = producto.ImagenUrl,
                        Categoria = new CategoriaDto
                        {
                            Id = producto.Categoria.Id,
                            Nombre = producto.Categoria.Nombre,
                            Descripcion = producto.Categoria.Descripcion,
                            ImagenUrl = producto.Categoria.ImagenUrl,

                        }
                    })
                    .ToListAsync();

                return Ok(new
                {
                    totalProductos,
                    pageNumber,
                    pageSize,
                    productos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener los productos.", details = ex.Message }); // 500 Internal Server Error
            }
        }


        [HttpPost]
        [Route("CrearProducto")]
        public async Task<IActionResult> CreateProduct([FromForm] CreateProductoDto createProductoDto)
        {
            try
            {
                // Obtener el rol desde el token (ya está validado por [Authorize])
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                // Validar si el usuario no es administrador
                if (role == "Cliente")
                {
                    return StatusCode(403, new { message = "Acceso denegado. Este recurso solo está disponible para administradores y Empleados." });
                }
                // Verificar si la categoría existe
                var categoria = await _context.Categorias.FindAsync(createProductoDto.CategoriaId);
                if (categoria == null)
                {
                    return BadRequest(new { message = "Categoría no encontrada." });
                }
                // Subir la imagen a Cloudinary
                string imageUrl = null;
                if (createProductoDto.Imagen != null)
                {
                    imageUrl = await _cloudinaryService.UploadImageAsync(createProductoDto.Imagen, "productos");
                }

                var producto = new Producto
                {
                    Nombre = createProductoDto.Nombre,
                    Precio = createProductoDto.Precio,
                    Descripcion = createProductoDto.Descripcion,
                    ImagenUrl = imageUrl,
                    CategoriaId = createProductoDto.CategoriaId // Relacionar el producto con la categoría
                };

                _context.Productos.Add(producto);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetSearchProduct), new { id = producto.Id },
                    new { message = "Producto añadido con éxito", producto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear el producto.", details = ex.Message });
            }
        }

        [HttpPut]
        [Route("EditarProducto/{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromForm] UpdateProductoDto updateProductoDto)
        {
            try
            {
                // Obtener el rol desde el token (ya está validado por [Authorize])
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                // Validar si el usuario no es administrador
                if (role != "Administrador")
                {
                    return StatusCode(403, new { message = "Acceso denegado. Este recurso solo está disponible para administradores." });
                }
                var producto = await _context.Productos.FindAsync(id);
                if (producto == null)
                {
                    return NotFound(new { message = "Producto no encontrado." });
                }
                // Verificar si la categoría existe antes de asignarla
                if (updateProductoDto.CategoriaId.HasValue)
                {
                    var categoria = await _context.Categorias.FindAsync(updateProductoDto.CategoriaId);
                    if (categoria == null)
                    {
                        return BadRequest(new { message = "Categoría no encontrada." });
                    }
                    producto.CategoriaId = updateProductoDto.CategoriaId.Value; // Asignar la nueva categoría
                }

                if (!string.IsNullOrEmpty(updateProductoDto.Nombre))
                    producto.Nombre = updateProductoDto.Nombre;
                if (updateProductoDto.Precio.HasValue)
                    producto.Precio = updateProductoDto.Precio.Value;
                if (!string.IsNullOrEmpty(updateProductoDto.Descripcion))
                    producto.Descripcion = updateProductoDto.Descripcion;

                // Subir la nueva imagen si se proporciona
                if (updateProductoDto.Imagen != null)
                {
                    // Eliminar la imagen anterior de Cloudinary
                    if (!string.IsNullOrEmpty(producto.ImagenUrl))
                    {

                        // Asegúrate de que publicId sea solo el ID de la imagen sin extensión
                        var publicId = producto.ImagenUrl.Split('/').Last().Split('.').First();

                        // Agregar la carpeta "productos/" de manera estática
                        var fullPublicId = $"productos/{publicId}";


                        // Aquí se pasa el publicId completo con la carpeta "productos/"
                        await _cloudinaryService.DeleteImageAsync(fullPublicId);
                    }

                    // Subir la nueva imagen a Cloudinary
                    producto.ImagenUrl = await _cloudinaryService.UploadImageAsync(updateProductoDto.Imagen, "productos");
                }

                _context.Productos.Update(producto);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Se ha actualizado el Producto: ", producto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el producto.", details = ex.Message });
            }
        }

        [HttpDelete]
        [Route("EliminarProducto/{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                // Obtener el rol desde el token (ya está validado por [Authorize])
                var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                // Validar si el usuario no es administrador
                if (role != "Administrador")
                {
                    return StatusCode(403, new { message = "Acceso denegado. Este recurso solo está disponible para administradores." });
                }
                var producto = await _context.Productos.FindAsync(id);

                if (producto == null)
                {
                    return NotFound(new { message = "Producto no encontrado." });
                }

                // Eliminar la imagen de Cloudinary si existe
                if (!string.IsNullOrEmpty(producto.ImagenUrl))
                {
                    // Asegúrate de que publicId sea solo el ID de la imagen sin extensión
                    var publicId = producto.ImagenUrl.Split('/').Last().Split('.').First();

                    // Agregar la carpeta "productos/" de manera estática
                    var fullPublicId = $"productos/{publicId}";


                    // Aquí se pasa el publicId completo con la carpeta "productos/"
                    await _cloudinaryService.DeleteImageAsync(fullPublicId);
                }

                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Se elimino el Producto : ", producto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar el producto.", details = ex.Message });
            }
        }
    }
}
