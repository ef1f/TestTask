using Microsoft.AspNetCore.Mvc;
using TestTask.Domain.Contracts;
using TestTask.Domain.Models;


namespace TestTransaction.API.Controllers
{
    /// <summary>
    /// Контроллер для управления финансовыми транзакциями
    /// </summary>
    [ApiController]
    [Route("/")]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _service;

        public TransactionsController(ITransactionService service)
            => _service = service ?? throw new ArgumentNullException(nameof(service));

        /// <summary>
        /// Выполнить операцию зачисления средств
        /// </summary>
        [HttpPost("credit")]
        public async Task<IActionResult> Credit([FromBody] CreditTransaction req, CancellationToken ct = default)
            => Ok(await _service.CreditAsync(req, ct));


        /// <summary>
        /// Выполнить операцию списания средств
        /// </summary>
        [HttpPost("debit")]
        public async Task<IActionResult> Debit([FromBody] DebitTransaction req, CancellationToken ct = default)
            => Ok(await _service.DebitAsync(req, ct));


        /// <summary>
        /// Отменить существующую транзакцию
        /// </summary>
        [HttpPost("revert")]
        public async Task<IActionResult> Revert([FromQuery] Guid id, CancellationToken ct = default)
            => Ok(await _service.RevertAsync(id, ct));


        /// <summary>
        /// Получить баланс клиента
        /// </summary>
        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance([FromQuery] Guid id, CancellationToken ct = default)
            => Ok(await _service.GetBalanceAsync(id, ct));
    }
}