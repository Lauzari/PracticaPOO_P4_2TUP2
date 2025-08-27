using Microsoft.AspNetCore.Mvc;
using Core.Entities;


namespace PracticaPOO_P4_2TUP2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankAccountController : ControllerBase
    {
        private static List<BankAccount> accounts = new List<BankAccount>();

        [HttpPost("create")]
        public ActionResult<BankAccount> CreateAccount([FromQuery] string owner, [FromQuery] decimal initialBalance)
        {
            try
            {
                var account = new BankAccount(owner, initialBalance);
                accounts.Add(account);
                return CreatedAtAction("GetAccountById", new { account.Id }, account);
            }
            catch (ArgumentOutOfRangeException e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("close-month")]
        public IActionResult CloseMonth([FromQuery] string type = null)
        {
            int processedCount = 0;

            foreach (var account in accounts)
            {
                if (type == null || account.GetType().Name == type)
                {
                    account.PerformMonthEndTransactions();
                    processedCount++;
                }
            }

            if (processedCount == 0)
                return NotFound($"No se encontraron cuentas del tipo {type} para procesar.");

            return Ok($"Cierre de mes aplicado a {processedCount} cuenta(s) de tipo {type ?? "todas"}.");
        }

        [HttpGet("all")]
        public ActionResult<IEnumerable<BankAccount>> GetAccounts()
        {
            return Ok(accounts);
        }

        [HttpGet("{accountId}")]
        public ActionResult<BankAccount> GetAccountById([FromRoute] int accountId)
        {
            var account = accounts.FirstOrDefault(a => a.Id == accountId);
            if (account == null)
                return NotFound($"No se encontró la cuenta con Id {accountId}.");

            return Ok(account);
        }

        [HttpGet("get/{number}")]
        public ActionResult<BankAccount> GetByNumber(string number)
        {
            var account = accounts.FirstOrDefault(a => a.Number == number);
            if (account == null)
                return NotFound($"No se encontró la cuenta con número {number}.");

            return Ok(account);
        }

        [HttpPost("withdraw")]
        public ActionResult<BankAccount> Withdraw([FromQuery] string accountNumber, [FromQuery] decimal amount)
        {
            var account = accounts.FirstOrDefault(a => a.Number == accountNumber);
            if (account == null)
                return NotFound($"No se encontró la cuenta con número {accountNumber}.");

            try
            {
                account.MakeWithdrawal(amount, DateTime.Now, "Retiro desde API");
                return Ok(account);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("deposit")]
        public ActionResult<BankAccount> Deposit([FromQuery] string accountNumber, [FromQuery] decimal amount)
        {
            var account = accounts.FirstOrDefault(a => a.Number == accountNumber);
            if (account == null)
                return NotFound($"No se encontró la cuenta con número {accountNumber}.");

            try
            {
                account.MakeDeposit(amount, DateTime.Now, "Depósito desde API");
                return Ok(account);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("history/{accountNumber}")]
        public ActionResult<string> GetHistory([FromRoute] string accountNumber)
        {
            var account = accounts.FirstOrDefault(a => a.Number == accountNumber);
            if (account == null)
                return NotFound($"No se encontró la cuenta con número {accountNumber}.");

            return Ok(account.GetAccountHistory());
        }

        [HttpGet("balance")]
        public ActionResult<string> GetBalance([FromQuery] string accountNumber)
        {
            try
            {
                var account = accounts.FirstOrDefault(a => a.Number == accountNumber);

                if (account == null)
                    return NotFound("Cuenta no encontrada.");

                return Ok($"El balance de la cuenta {account.Number} es ${account.Balance}.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}
