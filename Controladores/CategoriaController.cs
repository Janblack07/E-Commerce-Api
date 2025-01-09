using API_TIENDA.Servicios;
using E_Commerce_API.Dto;
using E_Commerce_API.Modelos;
using E_Commerce_API.Servicios;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_API.Controladores
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly  DbEcommerce _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ICloudinaryService _cloudinaryService;

        public CategoriaController(DbEcommerce context, IWebHostEnvironment environment, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _environment = environment;
            _cloudinaryService = cloudinaryService;

        }

        [HttpGet]
        
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categorias = _context.Categorias.Select(p => new CategoriaDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    ImagenUrl = p.ImagenUrl,

                }).ToList();

                return Ok(new { message = "Todos Las Categorias : ", categorias }); // 200 OK
            }
            catch (Exception ex)
            {
                // Manejo de error inesperado
                return StatusCode(500, new { message = "Error al obtener las categorias.", details = ex.Message }); // 500 Internal Server Error
            }
        }

        [HttpGet]
        [Route("BuscarCategoria/{id}")]
        public async Task<IActionResult> GetSearchCategories(int id)
        {
            try
            {
                var categorias = await _context.Categorias.FindAsync(id);

                if (categorias == null)
                {
                    return NotFound(new { message = "Categoria no encontrada." }); // 404 Not Found
                }

                var categoriaDto = new CategoriaDto
                {
                    Id = categorias.Id,
                    Nombre = categorias.Nombre,
                    Descripcion = categorias.Descripcion,
                    ImagenUrl = categorias.ImagenUrl

                };

                return Ok(new { message = " La Categoria es :", categoriaDto }); // 200 OK
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la categoria.", details = ex.Message }); // 500 Internal Server Error
            }
        }

        [HttpGet]
        [Route("BuscarCategoriaNombre/{nombre}")]
        public async Task<IActionResult> GetSearchCategoriesByName(string nombre)
        {
            try
            {
                // Buscar la categoría por nombre (ignorando mayúsculas/minúsculas)
                var categoria = await _context.Categorias
                    .FirstOrDefaultAsync(c => c.Nombre.ToLower() == nombre.ToLower());

                if (categoria == null)
                {
                    return NotFound(new { message = "Categoría no encontrada." }); // 404 Not Found
                }

                // Mapear la categoría al DTO
                var categoriaDto = new CategoriaDto
                {
                    Id = categoria.Id,
                    Nombre = categoria.Nombre,
                    Descripcion = categoria.Descripcion,
                    ImagenUrl = categoria.ImagenUrl
                };

                return Ok(new { message = "La categoría encontrada es:", categoriaDto }); // 200 OK
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al obtener la categoría.", details = ex.Message }); // 500 Internal Server Error
            }
        }

        [HttpPost]
        [Route("CrearCategoria")]
        public async Task<IActionResult> CreateCategories([FromForm] CreateCategoriaDto createCategoriaDto)
        {
            try
            {

                // Subir la imagen a Cloudinary
                string imageUrl = null;
                if (createCategoriaDto.Imagen != null)
                {
                    imageUrl = await _cloudinaryService.UploadImageAsync(createCategoriaDto.Imagen, "categoria");
                }

                var categoria = new Categoria
                {
                    Nombre = createCategoriaDto.Nombre,
                    Descripcion = createCategoriaDto.Descripcion,
                    ImagenUrl = imageUrl,

                };

                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetSearchCategories), new { id = categoria.Id },
                    new { message = "Categoria añadida con éxito", categoria });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al crear la categoria.", details = ex.Message });
            }
        }

        [HttpPut]
        [Route("EditarCategoria/{id}")]
        public async Task<IActionResult> UpdateCategories(int id, [FromForm] UpdateCategoriaDto updateCategoriaDto)
        {
            try
            {
                // Buscar la categoría a actualizar
                var categoria = await _context.Categorias.FindAsync(id);
                if (categoria == null)
                {
                    return NotFound(new { message = "Categoría no encontrada." }); // 404 Not Found
                }

                // Actualizar los campos de la categoría si se proporcionan valores
                if (!string.IsNullOrEmpty(updateCategoriaDto.Nombre))
                    categoria.Nombre = updateCategoriaDto.Nombre;
                if (!string.IsNullOrEmpty(updateCategoriaDto.Descripcion))
                    categoria.Descripcion = updateCategoriaDto.Descripcion;

                // Subir la nueva imagen si se proporciona
                if (updateCategoriaDto.Imagen != null)
                {
                    // Eliminar la imagen anterior de Cloudinary si existe
                    if (!string.IsNullOrEmpty(categoria.ImagenUrl))
                    {
                        var publicId = categoria.ImagenUrl.Split('/').Last().Split('.').First();
                        var fullPublicId = $"categoria/{publicId}";
                        await _cloudinaryService.DeleteImageAsync(fullPublicId); // Eliminar imagen anterior
                    }

                    // Subir la nueva imagen a Cloudinary
                    categoria.ImagenUrl = await _cloudinaryService.UploadImageAsync(updateCategoriaDto.Imagen, "categoria");
                }

                // Guardar los cambios en la base de datos
                _context.Categorias.Update(categoria);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Categoría actualizada correctamente.", categoria });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar la categoría.", details = ex.Message });
            }
        }

        [HttpDelete]
        [Route("EliminarCategoria/{id}")]
        public async Task<IActionResult> DeleteCategories(int id)
        {
            try
            {
                // Buscar la categoría a eliminar
                var categoria = await _context.Categorias.FindAsync(id);
                if (categoria == null)
                {
                    return NotFound(new { message = "Categoría no encontrada." }); // 404 Not Found
                }

                // Eliminar la imagen de Cloudinary si existe
                if (!string.IsNullOrEmpty(categoria.ImagenUrl))
                {
                    var publicId = categoria.ImagenUrl.Split('/').Last().Split('.').First();
                    var fullPublicId = $"categoria/{publicId}";
                    await _cloudinaryService.DeleteImageAsync(fullPublicId); // Eliminar imagen de Cloudinary
                }

                // Eliminar la categoría de la base de datos
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Categoría eliminada correctamente.", categoria });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al eliminar la categoría.", details = ex.Message });
            }
        }
    }
}
