using Microsoft.AspNetCore.Mvc;
using Warehouse.Application.Interfaces;
using Warehouse.Application.Models.Filters;
using Warehouse.Domain.Entities;

namespace Warehouse.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BalancesController : ControllerBase
    {
        private readonly IBalanceService _service;

        public BalancesController(IBalanceService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Balance>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var balances = await _service.GetAllAsync();
            return Ok(balances);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Balance), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int id)
        {
            var balance = await _service.GetByIdAsync(id);
            return balance is null ? NotFound() : Ok(balance);
        }

        [HttpGet("filter")]
        [ProducesResponseType(typeof(IEnumerable<Balance>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetFiltered([FromQuery] BalanceFilter filter)
        {
            var results = await _service.GetFilteredAsync(filter);
            return Ok(results);
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
