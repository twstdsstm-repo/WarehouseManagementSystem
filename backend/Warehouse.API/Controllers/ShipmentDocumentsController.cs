using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Application.Models.Filters;

namespace Warehouse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShipmentDocumentsController : ControllerBase
    {
        private readonly IShipmentDocumentService _service;

        public ShipmentDocumentsController(IShipmentDocumentService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ShipmentDocument>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var docs = await _service.GetAllAsync();
            return Ok(docs);
        }

        [HttpGet("filter")]
        [ProducesResponseType(typeof(IEnumerable<ShipmentDocument>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFiltered([FromQuery] ShipmentDocumentFilter filter)
        {
            var result = await _service.GetFilteredAsync(filter);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ShipmentDocument), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var doc = await _service.GetByIdAsync(id);
            return doc is null ? NotFound() : Ok(doc);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ShipmentDocument), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ShipmentDocument document)
        {
            var id = await _service.CreateAsync(document);
            return CreatedAtAction(nameof(GetById), new { id }, document);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] ShipmentDocument document)
        {
            if (document == null)
                return BadRequest("Тело запроса не может быть пустым.");

            if (document.Id != 0 && document.Id != id)
                return BadRequest("ID в пути и теле запроса не совпадают.");

            var updated = await _service.UpdateAsync(id, document);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpPost("{id:int}/sign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Sign(int id)
        {
            try
            {
                var success = await _service.SignDocumentAsync(id);
                return success ? Ok("Документ успешно подписан.") : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id:int}/revoke")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Revoke(int id)
        {
            try
            {
                var success = await _service.RevokeDocumentAsync(id);
                return success ? Ok("Документ успешно отозван.") : NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{documentId:int}/resources/{resourceId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteResourceFromDocument(int documentId, int resourceId)
        {
            var result = await _service.DeleteResourceFromDocumentAsync(documentId, resourceId);

            if (!result)
                return NotFound("Ресурс не найден в документе отгрузки.");

            return NoContent();
        }
    }
}
