using AutoMapper;
using GloboTicket.TicketManagement.Application.Contracts.Infrastructure;
using GloboTicket.TicketManagement.Application.Contracts.Persistence;
using GloboTicket.TicketManagement.Application.Models.Mail;
using GloboTicket.TicketManagement.Domain.Entities;
using MediatR;

namespace GloboTicket.TicketManagement.Application.Features.Events.Commands.CreateEvent
{
    public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Guid> //return the ID of the newly created event 
    {
        private readonly IEventRepository _eventRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;


        public CreateEventCommandHandler(IMapper mapper, IEventRepository eventRepository, IEmailService emailService)
        {
            _mapper = mapper;
            _eventRepository = eventRepository;
            _emailService = emailService;
        }

        public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
        {
            // Adding validation Logic
            var validator = new CreateEventCommandValidator(_eventRepository); //instintate the cutom validation for the Request 
            var validationResult = await validator.ValidateAsync(request);      // receive list of errors 

            if (validationResult.Errors.Count > 0)                // if there an exception throw it
                throw new Exceptions.ValidationException(validationResult);

            var @event = _mapper.Map<Event>(request); // map the request to the model 


            @event = await _eventRepository.AddAsync(@event);    // store the event 

            //when event is create Send AnEmail Notification to the user 


            var email = new Email()
            {
                To = "gill@snowball.be",
                Body = $"Anew Event Was Created : {request}",
                Subject = "A new Event was Created "
            };

            try
            {
                await _emailService.SendEmail(email);
            }catch (Exception e)
            {
                //this shouldn't stop APIfrom doing else so can be logged 
            }

            return @event.EventId;   //return the id of the newly added event 
        }
    }
}