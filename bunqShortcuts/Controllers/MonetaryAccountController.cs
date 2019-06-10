using System.Collections.Generic;
using Bunq.Sdk.Context;
using Bunq.Sdk.Model.Generated.Endpoint;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bunqShortcuts.Controllers
{
    public class Account
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Balance { get; set; }
    }

    [Route("api/[controller]")]
    public class MonetaryAccountController : Controller
    {
        // GET: api/MonetaryAccount
        [HttpGet]
        [Authorize]
        public JsonResult Get()
        {
            var apiContext = ApiContext.Restore();
            BunqContext.LoadApiContext(apiContext);

            var monetaryAccounts = MonetaryAccount.List();
            var accounts = new List<Account>();
            foreach (var acc in monetaryAccounts.Value) {
                if (acc.MonetaryAccountBank != null) {
                    var account = new Account {
                        Id = acc.MonetaryAccountBank.Id.ToString(),
                        Name = acc.MonetaryAccountBank.Description,
                        Balance = $"€{acc.MonetaryAccountBank.Balance.Value}"
                    };
                    accounts.Add(account);
                }
            }
            return Json(accounts);
        }

        // GET api/monetaryaccount/ID
        [HttpGet("{id}")]
        [Authorize]
        public ActionResult Get(int id)
        {
            var apiContext = ApiContext.Restore();
            BunqContext.LoadApiContext(apiContext);

            var monetaryAccount = MonetaryAccount.Get(id);
            if (monetaryAccount.Value.MonetaryAccountBank == null) {
                return BadRequest();
            }

            var account = new Account {
                Id = monetaryAccount.Value.MonetaryAccountBank.Id.ToString(),
                Name = monetaryAccount.Value.MonetaryAccountBank.Description,
                Balance = $"€{monetaryAccount.Value.MonetaryAccountBank.Balance.Value}"
            };

            return Json(account);
        }
    }
}
