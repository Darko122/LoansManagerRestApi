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

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            var loan = await loansService.GetAsync(id);

            if (loan == null)
                return BadRequest(ValidationResultFactory(nameof(id), id, LoansControllerResources.LoanDoesNotExist, id.ToString()));

            return Ok(loan);
        }

        [HttpGet]
        [Route("getLimited")]
        public async Task<IActionResult> GetAsync([FromQuery(Name = "offset")] int offset = 0, [FromQuery(Name = "take")] int take = 15)
        {
            if (take > apiSettings.MaxNumberOfRecordToGet)
                return BadRequest(ValidationResultFactory(nameof(take), take, UserControllerResources.MaxNumberOfRecordToGetExceeded, apiSettings.MaxNumberOfRecordToGet.ToString()));

            var loans = await loansService.GetAsync(offset, take);
            if (loans.Any())
                return Ok(loans);

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(CreateLoanCommand createLoanCommand)
        {
            var validationResult = await commandBus.Validate(createLoanCommand);
            if (!validationResult.IsValid)
                return BadRequest(validationResult);

            createLoanCommand.Id = Guid.NewGuid();
            await commandBus.Submit(createLoanCommand);

            return Created($"users/{createLoanCommand.Id}", createLoanCommand);
        }
    }
}