using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Interfaces;
using Warehouse.Domain.Entities;
using Warehouse.Application.Models.Filters;

namespace Warehouse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptDocumentsController : ControllerBase
    {
        private readonly IReceiptDocumentService _service;
        private readonly IReceiptResourceService _receiptResourceService;

        public ReceiptDocumentsController(IReceiptDocumentService service, IReceiptResourceService receiptResourceService)
        {
            _service = service;
            _receiptResourceService = receiptResourceService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ReceiptDocument>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _service.GetAllAsync());
        }

        [HttpGet("filter")]
        [ProducesResponseType(typeof(IEnumerable<ReceiptDocument>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFiltered([FromQuery] ReceiptDocumentFilter filter)
        {
            return Ok(await _service.GetFilteredAsync(filter));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ReceiptDocument), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var doc = await _service.GetByIdAsync(id);
            return doc is null ? NotFound() : Ok(doc);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ReceiptDocument), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] ReceiptDocument document)
        {
            var id = await _service.CreateAsync(document);
            return CreatedAtAction(nameof(GetById), new { id }, document);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] ReceiptDocument document)
        {
            if (document == null) return BadRequest("Пустое тело запроса.");
            if (document.Id != 0 && document.Id != id)
                return BadRequest("Идентификатор в пути и теле запроса не совпадают.");

            var updated = await _service.UpdateAsync(id, document);
            return updated ? NoContent() : NotFound();
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            return deleted ? NoContent() : NotFound();
        }

        [HttpGet("{id:int}/resources")]
        [ProducesResponseType(typeof(IEnumerable<ReceiptResource>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetResourcesByDocumentId(int id)
        {

            var resources = await _receiptResourceService.GetByDocumentIdAsync(id);

            if (resources == null || !resources.Any())
            {
                return NotFound();
            }

            return Ok(resources);
        }

        [HttpDelete("{documentId:int}/resources/{resourceId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteResourceFromDocument(int documentId, int resourceId)
        {

            var result = await _service.DeleteResourceFromDocumentAsync(documentId, resourceId);

            if (!result)
            {
                return NotFound("Ресурс не найден в документе.");
            }

            return NoContent();
        }
    }
}
