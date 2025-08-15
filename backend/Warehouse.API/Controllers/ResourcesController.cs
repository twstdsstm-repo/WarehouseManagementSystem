using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;

namespace Warehouse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResourcesController : ControllerBase
    {
        private readonly IResourceService _service;

        public ResourcesController(IResourceService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Resource>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var res = await _service.GetAllAsync();
            return Ok(res);
        }

        [HttpGet("api/resources/{id}")]
        [ProducesResponseType(typeof(Resource), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var resource = await _service.GetByIdAsync(id);
            return resource is null ? NotFound() : Ok(resource);
        }


        [HttpPost]
        [ProducesResponseType(typeof(Resource), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Resource resource)
        {
            var id = await _service.CreateAsync(resource);
            return CreatedAtAction(nameof(GetById), new { id }, resource);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Resource resource)
        {
            if (resource == null) return BadRequest("Пустое тело запроса.");
            if (resource.Id != 0 && resource.Id != id)
                return BadRequest("Идентификатор в пути и теле запроса не совпадают.");

            var updated = await _service.UpdateAsync(id, resource);
            return updated ? NoContent() : NotFound();
        }

        [HttpPost("{id:int}/archive")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Archive(int id)
        {
            var ok = await _service.ArchiveAsync(id);
            return ok ? Ok() : NotFound();
        }

        [HttpPost("{id:int}/unarchive")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Unarchive(int id)
        {
            try
            {
                
                await _service.UnarchiveAsync(id);
                return Ok(); 
            }
            catch (KeyNotFoundException)
            {
                
                return NotFound();
            }
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }
    }
}
