//***********************************************************************************
//Program: EventParticipant.cs
//Description: Event participant model
//Date: Oct 12, 2025
//Author: John Nasitem
//***********************************************************************************



namespace takethetab_server.Domain.Entities
{
    public class EventParticipant(User participant, Event parentEvent, List<EventParticipantDebt> debts)
    {
        /// <summary>
        /// User participanting in the event
        /// </summary>
        public User Participant { get; set; } = participant;



        /// <summary>
        /// Event the user is participanting in
        /// </summary>
        public Event ParentEvent { get; set; } = parentEvent;



        /// <summary>
        /// Debts this user owes to other participants
        /// </summary>
        public List<EventParticipantDebt> Debts { get; set; } = debts;
    }



    public class EventParticipantDebt
    {
        /// <summary>
        /// User this user owes money to
        /// </summary>
        public User CreditorUser { get; set; } = null!;



        /// <summary>
        /// Has this user paid back the debt
        /// </summary>
        public bool HasPaidBack { get; set; }



        /// <summary>
        /// Has the creditor user confirmed that they recieved the payment
        /// </summary>
        public bool PaymnetConfirmed { get; set; }



        /// <summary>
        /// The amount this user owes
        /// </summary>
        public decimal DebtAmount { get; set; }
    }
}
