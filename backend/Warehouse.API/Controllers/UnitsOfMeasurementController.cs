using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;

namespace Warehouse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UnitsOfMeasurementController : ControllerBase
    {
        private readonly IUnitOfMeasurementService _service;

        public UnitsOfMeasurementController(IUnitOfMeasurementService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UnitOfMeasurement>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var units = await _service.GetAllAsync();
            return Ok(units);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(UnitOfMeasurement), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var unit = await _service.GetByIdAsync(id);
            return unit is null ? NotFound() : Ok(unit);
        }

        [HttpPost]
        [ProducesResponseType(typeof(UnitOfMeasurement), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] UnitOfMeasurement unit)
        {
            var id = await _service.CreateAsync(unit);
            return CreatedAtAction(nameof(GetById), new { id }, unit);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] UnitOfMeasurement unit)
        {
            if (unit == null) return BadRequest("Пустое тело запроса.");
            if (unit.Id != 0 && unit.Id != id)
                return BadRequest("Идентификатор в пути и теле запроса не совпадают.");

            var updated = await _service.UpdateAsync(id, unit);
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
