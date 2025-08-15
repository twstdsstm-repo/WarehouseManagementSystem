using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;

namespace Warehouse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _service;

        public ClientsController(IClientService service)
        {
            _service = service;
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Client>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var clients = await _service.GetAllAsync();
            return Ok(clients);
        }
        
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Client), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var client = await _service.GetByIdAsync(id);
            return client is null ? NotFound() : Ok(client);
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(Client), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Client client)
        {
            var id = await _service.CreateAsync(client); 
            return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
        }
        
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Client client)
        {
            if (client == null) return BadRequest("Пустое тело запроса.");
            if (client.Id != 0 && client.Id != id)
                return BadRequest("Идентификатор в пути и теле запроса не совпадают.");

            var updated = await _service.UpdateAsync(id, client); 
            return updated is null ? NotFound() : NoContent();
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
