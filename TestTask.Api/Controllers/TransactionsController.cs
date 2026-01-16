using Microsoft.AspNetCore.Mvc;
using TestTask.Application.Interfaces;
using TestTask.Core.Models;


namespace TestTask.Api.Controllers
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
        public async Task<ActionResult<TransactionResponse>> Credit([FromBody] CreditTransaction req,
            CancellationToken ct = default)
            => Ok(await _service.CreditAsync(req, ct));


        /// <summary>
        /// Выполнить операцию списания средств
        /// </summary>
        [HttpPost("debit")]
        public async Task<ActionResult<TransactionResponse>> Debit([FromBody] DebitTransaction req,
            CancellationToken ct = default)
            => Ok(await _service.DebitAsync(req, ct));


        /// <summary>
        /// Отменить существующую транзакцию
        /// </summary>
        [HttpPost("revert")]
        public async Task<ActionResult<RevertResponse>> Revert([FromQuery] Guid id, CancellationToken ct = default)
            => Ok(await _service.RevertAsync(id, ct));


        /// <summary>
        /// Получить баланс клиента
        /// </summary>
        [HttpGet("balance")]
        public async Task<ActionResult<BalanceResponse>> GetBalance([FromQuery] Guid id, CancellationToken ct = default)
            => Ok(await _service.GetBalanceAsync(id, ct));
    }
}