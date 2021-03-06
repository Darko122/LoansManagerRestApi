﻿using LoansManager.Resources;
using LoansManager.Services.Commands;
using LoansManager.Services.Infrastructure.CommandsSetup;
using LoansManager.Services.Infrastructure.SettingsModels;
using LoansManager.Services.ServicesContracts;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LoansManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoansController : ApplicationBaseController
    {
        private readonly ICommandBus commandBus;
        private readonly ILoansService loansService;
        private readonly ApiSettings apiSettings;

        public LoansController(
            ICommandBus commandBus,
            ILoansService loansService,
            ApiSettings apiSettings
            )
        {
            this.commandBus = commandBus;
            this.loansService = loansService;
            this.apiSettings = apiSettings;
        }

        /// <summary>
        /// Gets loan by its <paramref name="id"/> key.
        /// </summary>
        /// <param name="id">Key of concrete loan.</param>
        /// <returns>Loan object.</returns>
        /// <response code="200">When loan found.</response>
        /// <response code="404">When no loan found.</response> 
        [HttpGet]
        [Route("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var loan = await loansService.GetAsync(id);

            if (loan != null)
                return Ok(loan);

            return NotFound(id);
        }

        /// <summary>
        /// Gets collection of loans.
        /// </summary>
        /// <param name="offset">Offset from first record.</param>
        /// <param name="take">Amount of records to take.</param>
        /// <returns>Collection of loans objects.</returns>
        /// <response code="200">When at least one record found.</response>
        /// <response code="400">When to many records requested.</response> 
        /// <response code="404">When no records found.</response> 
        [HttpGet]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetAsync([FromQuery(Name = "offset")] int offset = 0, [FromQuery(Name = "take")] int take = 15)
        {
            if (take > apiSettings.MaxNumberOfRecordToGet)
                return BadRequest(ValidationResultFactory(nameof(take), take, UserControllerResources.MaxNumberOfRecordToGetExceeded, apiSettings.MaxNumberOfRecordToGet.ToString()));

            var loans = await loansService.GetAsync(offset, take);
            if (loans.Any())
                return Ok(loans);

            return NotFound();
        }

        /// <summary>
        /// Gets list of cash borrowers.
        /// </summary>
        /// <param name="offset">Offset from first record.</param>
        /// <param name="take">Amount of records to take.</param>
        /// <returns>Collection of borrowers ids.</returns>
        /// <response code="200">When at least one record found.</response>
        /// <response code="400">When to many records requested.</response> 
        /// <response code="404">When no records found.</response> 
        [HttpGet]
        [Route("Borrowers")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetBorrowersAsync([FromQuery(Name = "offset")] int offset = 0, [FromQuery(Name = "take")] int take = 15)
        {
            if (take > apiSettings.MaxNumberOfRecordToGet)
                return BadRequest(ValidationResultFactory(nameof(take), take, UserControllerResources.MaxNumberOfRecordToGetExceeded, apiSettings.MaxNumberOfRecordToGet.ToString()));

            var users = await loansService.GetBorrowersAsync(offset, take);
            if (users.Any())
                return Ok(users);

            return NotFound();
        }

        /// <summary>
        /// Gets list of cash lenders.
        /// </summary>
        /// <param name="offset">Offset from first record.</param>
        /// <param name="take">Amount of records to take.</param>
        /// <returns>Collection of lenders ids.</returns>
        /// <response code="200">When at least one record found.</response>
        /// <response code="400">When to many records requested.</response> 
        /// <response code="404">When no records found.</response> 
        [HttpGet]
        [Route("Lenders")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetLendersAsync([FromQuery(Name = "offset")] int offset = 0, [FromQuery(Name = "take")] int take = 15)
        {
            if (take > apiSettings.MaxNumberOfRecordToGet)
                return BadRequest(ValidationResultFactory(nameof(take), take, UserControllerResources.MaxNumberOfRecordToGetExceeded, apiSettings.MaxNumberOfRecordToGet.ToString()));

            var users = await loansService.GetLendersAsync(offset, take);
            if (users.Any())
                return Ok(users);

            return NotFound();
        }

        /// <summary>
        /// Gets collection of loans to pay off by specified user.
        /// </summary>
        /// <param name="userId">User who borrowed cash.</param>
        /// <param name="offset">Offset from first record.</param>
        /// <param name="take">Amount of records to take.</param>
        /// <returns>Collection of loans.</returns>
        /// <response code="200">When at least one record found.</response>
        /// <response code="400">When to many records requested.</response> 
        /// <response code="404">When no records found.</response> 
        [HttpGet]
        [Route("{userId}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserLoansAsync(string userId, [FromQuery(Name = "offset")] int offset = 0, [FromQuery(Name = "take")] int take = 15)
        {
            if (take > apiSettings.MaxNumberOfRecordToGet)
                return BadRequest(ValidationResultFactory(nameof(take), take, UserControllerResources.MaxNumberOfRecordToGetExceeded, apiSettings.MaxNumberOfRecordToGet.ToString()));

            var loans = await loansService.GetUserLoansAsync(userId, offset, take);
            if (loans.Any())
                return Ok(loans);

            return NotFound();
        }

        /// <summary>
        /// Adds new loan defined by model.
        /// </summary>
        /// <param name="createLoanCommand">Loan model.</param>
        /// <response code="201">When loan created.</response>
        /// <response code="400">When validation on <paramref name="createLoanCommand"/> failed.</response>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateAsync([FromBody]CreateLoanCommand createLoanCommand)
        {
            var validationResult = await commandBus.Validate(createLoanCommand);
            if (!validationResult.IsValid)
                return BadRequest(validationResult);
            
            await commandBus.Submit(createLoanCommand);
            return Created($"users/{createLoanCommand.Id}", createLoanCommand);
        }

        /// <summary>
        /// Repays specified loan by model.
        /// </summary>
        /// <param name="repayLoanCommand">Loan model.</param>
        /// <response code="202">When accepted loan repaid.</response>
        /// <response code="400">When validation on <paramref name="repayLoanCommand"/> failed.</response>
        [HttpPatch]
        [Route("Repay")]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RepayAsync([FromBody]RepayLoanCommand repayLoanCommand)
        {
            var validationResult = await commandBus.Validate(repayLoanCommand);
            if (!validationResult.IsValid)
                return BadRequest(validationResult);
            
            await commandBus.Submit(repayLoanCommand);
            return Accepted(repayLoanCommand);
        }
    }
}
