﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kledex.Domain;
using Kledex.Store.EF.Entities.Factories;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Kledex.Store.EF.Stores
{
    /// <inheritdoc />
    public class CommandStore : ICommandStore
    {
        private readonly IDomainDbContextFactory _dbContextFactory;
        private readonly ICommandEntityFactory _commandEntityFactory;

        public CommandStore(IDomainDbContextFactory dbContextFactory, ICommandEntityFactory commandEntityFactory)
        {
            _dbContextFactory = dbContextFactory;
            _commandEntityFactory = commandEntityFactory;            
        }

        /// <inheritdoc />
        public async Task SaveCommandAsync(IDomainCommand command)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var newCommandEntity = _commandEntityFactory.CreateCommand(command);
                await dbContext.Commands.AddAsync(newCommandEntity);
                await dbContext.SaveChangesAsync();
            }
        }

        /// <inheritdoc />
        public void SaveCommand(IDomainCommand command)
        {
            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var newCommandEntity = _commandEntityFactory.CreateCommand(command);
                dbContext.Commands.Add(newCommandEntity);
                dbContext.SaveChanges();
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDomainCommand>> GetCommandsAsync(Guid aggregateId)
        {
            var result = new List<IDomainCommand>();

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var commands = await dbContext.Commands.Where(x => x.AggregateId == aggregateId).ToListAsync();
                foreach (var command in commands)
                {
                    var domainCommand = JsonConvert.DeserializeObject(command.Data, Type.GetType(command.Type));
                    result.Add((IDomainCommand)domainCommand);
                }
            }

            return result;
        }

        /// <inheritdoc />
        public IEnumerable<IDomainCommand> GetCommands(Guid aggregateId)
        {
            var result = new List<IDomainCommand>();

            using (var dbContext = _dbContextFactory.CreateDbContext())
            {
                var commands = dbContext.Commands.Where(x => x.AggregateId == aggregateId).ToList();
                foreach (var command in commands)
                {
                    var domainCommand = JsonConvert.DeserializeObject(command.Data, Type.GetType(command.Type));
                    result.Add((IDomainCommand)domainCommand);
                }
            }

            return result;
        }
    }
}
