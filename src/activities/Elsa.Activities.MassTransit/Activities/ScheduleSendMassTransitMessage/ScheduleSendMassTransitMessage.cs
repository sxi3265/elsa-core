using System;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Activities.MassTransit.Options;
using Elsa.ActivityResults;
using Elsa.Attributes;
using Elsa.Services.Models;
using MassTransit;
using MassTransit.Scheduling;
using Microsoft.Extensions.Options;
using NodaTime;

// ReSharper disable once CheckNamespace
namespace Elsa.Activities.MassTransit
{
    [ActivityDefinition(
        Category = "MassTransit",
        DisplayName = "Schedule MassTransit Message",
        Description = "Schedule a message via MassTransit."
    )]
    public class ScheduleSendMassTransitMessage : MassTransitBusActivity
    {
        private readonly MessageScheduleOptions options;

        public ScheduleSendMassTransitMessage(IBus bus,
            ConsumeContext consumeContext,
            IOptions<MessageScheduleOptions> options)
            : base(bus, consumeContext)
        {
            this.options = options.Value;
        }

        [ActivityProperty(Hint = "An expression that evaluates to the message to be delivered.")]
        public object? Message { get; set; }

        [ActivityProperty(Hint = "The address of a specific endpoint to deliver the message to.")]
        public Uri EndpointAddress { get; set; }

        [ActivityProperty(Hint = "An expression that evaluates to the date and time to deliver the message.")]
        public Instant ScheduledTime { get; set; }

        protected override bool OnCanExecute(ActivityExecutionContext context) =>
            Message != null && options.SchedulerAddress != null;

        protected override async Task<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context,
            CancellationToken cancellationToken)
        {
            var endpoint = await SendEndpointProvider.GetSendEndpoint(options.SchedulerAddress);
            var scheduledMessage = await endpoint.ScheduleSend(
                EndpointAddress,
                ScheduledTime.ToDateTimeUtc(),
                Message,
                cancellationToken);

            return Done(scheduledMessage.TokenId);
        }
    }
}