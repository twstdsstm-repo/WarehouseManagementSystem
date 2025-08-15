using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;

namespace Warehouse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentResourcesController : ControllerBase
    {
        private readonly IShipmentResourceService _service;

        public ShipmentResourcesController(IShipmentResourceService service)
        {
            _service = service;
        }
        
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ShipmentResource>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var list = await _service.GetAllAsync();
            return Ok(list);
        }
        
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ShipmentResource), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(ShipmentResource), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ShipmentResource resource)
        {
            if (resource == null)
                return BadRequest("Данные ресурса отгрузки не переданы.");

            var id = await _service.CreateAsync(resource);
            return CreatedAtAction(nameof(GetById), new { id }, resource);
        }
        
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] ShipmentResource resource)
        {
            if (resource == null)
                return BadRequest("Данные ресурса отгрузки не переданы.");

            if (resource.Id != 0 && resource.Id != id)
                return BadRequest("ID в теле и URL не совпадают.");

            var success = await _service.UpdateAsync(id, resource);
            return success ? NoContent() : NotFound();
        }
        
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            return success ? NoContent() : NotFound();
        }
    }
}
