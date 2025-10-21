//***********************************************************************************
//Program: Event.cs
//Description: Event model
//Date: Sep 25, 2025
//Author: John Nasitem
//***********************************************************************************



namespace takethetab_server.Domain.Entities
{
    public class Event(long id, User creator, string name, DateTime date, List<Activity> activities, List<User> participants)
    {
        /// <summary>
        /// Id of event
        /// </summary>
        public long Id { get; set; } = id;



        /// <summary>
        /// User who created the event
        /// </summary>
        public User Creator { get; set; } = creator;



        /// <summary>
        /// Name of event
        /// </summary>
        public string Name { get; set; } = name;



        /// <summary>
        /// Date event took place on
        /// </summary>
        public DateTime Date { get; set; } = date;



        /// <summary>
        /// Activities under the event
        /// </summary>
        public List<Activity> Activities { get; set; } = activities;



        /// <summary>
        /// Users who participated in the event
        /// </summary>
        public List<User> Participants = participants;



        /// <summary>
        /// Get the total amount owed to the specified user
        /// </summary>
        /// <param name="userId">Id of user</param>
        /// <returns>Total amount owed</returns>
        public decimal GetUserTotalOwed(long userId)
        {
            decimal totalOwed = 0;

            foreach (User particiant in Participants)
            {
                if (particiant.Id == userId)
                    continue;

                decimal netDiff = GetNetAmountBetweenUsers(userId, particiant.Id);

                if (netDiff > 0)
                    totalOwed += netDiff;
            }

            if (Creator.Id != userId)
            {
                decimal netDiff = GetNetAmountBetweenUsers(userId, Creator.Id);

                if (netDiff > 0)
                    totalOwed += netDiff;
            }


            return totalOwed;
        }



        /// <summary>
        /// Get the total amount the specified user owes
        /// </summary>
        /// <param name="userId">Id of user</param>
        /// <returns>Total amount owing</returns>
        public decimal GetUserTotalOwing(long userId)
        {
            decimal totalOwing = 0;

            foreach (User particiant in Participants)
            {
                if (particiant.Id == userId)
                    continue;

                decimal netDiff = GetNetAmountBetweenUsers(particiant.Id, userId);

                if (netDiff > 0)
                    totalOwing += netDiff;
            }

            if (Creator.Id != userId)
            {
                decimal netDiff = GetNetAmountBetweenUsers(Creator.Id, userId);

                if (netDiff > 0)
                    totalOwing += netDiff;
            }

            return totalOwing;
        }



        /// <summary>
        /// Has the specified payer paid everything back to the specified creditor
        /// </summary>
        /// <param name="creditorId">Id of creditor user</param>
        /// <param name="payerId">Id of payer user</param>
        /// <returns>True if payer has claimed to sent all debts, otherwise false</returns>
        public bool HasPayerSettledDebt(long creditorId, long payerId)
        {
            return Activities
                .Where(a => a.Payee.Id == creditorId)
                .All(a => a.HasPayerFullyRepaid(payerId));
        }



        /// <summary>
        /// Has the specified creditor confirmed they recieved payment from the specified payer
        /// </summary>
        /// <param name="creditorId">Id of creditor user</param>
        /// <param name="payerId">Id of payer user</param>
        /// <returns>True if all payments were confirmed, otherwise false</returns>
        public bool HasCreditorConfirmedPayments(long creditorId, long payerId)
        {
            return Activities
                .Where(a => a.Payee.Id == creditorId)
                .All(a => a.HasPaymentsBeenConfirmed(payerId));
        }



        /// <summary>
        /// Get the net difference in amount owed between two users
        /// </summary>
        /// <param name="userA">First user</param>
        /// <param name="userB">Second user</param>
        /// <returns>How much user b owes to user a</returns>
        public decimal GetNetAmountBetweenUsers(long userA, long userB)
        {
            decimal totalOwedByB = 0;
            decimal totalOwedByA = 0;

            foreach (Activity activity in Activities)
            {
                if (activity.Payee.Id != userA && activity.Payee.Id != userB)
                    continue;

                decimal activityTotalOwedByA = 0;
                decimal activityTotalOwedByB = 0;

                foreach (ActivityItemPayer itemPayer in activity.Items.SelectMany(i => i.Payers))
                {
                    if (itemPayer.PaymentConfirmed)
                        continue;

                    if (itemPayer.Payer.Id == userA && activity.Payee.Id == userB)
                        activityTotalOwedByA += itemPayer.AmountOwing;

                    else if (itemPayer.Payer.Id == userB && activity.Payee.Id == userA)
                        activityTotalOwedByB += itemPayer.AmountOwing;
                }

                int payerCount = activity.Items.SelectMany(i => i.Payers)
                                  .Select(p => p.Payer.Id)
                                  .Distinct()
                                  .Count();

                decimal taxAmountA = activity.AddFivePercentTax ? activityTotalOwedByA * 0.05m : 0;
                decimal gratuityAmountA = activity.IsGratuityTypePercent ? activityTotalOwedByA * (activity.GratuityAmount / 100m) : activity.GratuityAmount / payerCount;
                decimal taxAmountB = activity.AddFivePercentTax ? activityTotalOwedByB * 0.05m : 0;
                decimal gratuityAmountB = activity.IsGratuityTypePercent ? activityTotalOwedByB * (activity.GratuityAmount / 100m) : activity.GratuityAmount / payerCount;

                totalOwedByB += activityTotalOwedByB != 0 ? activityTotalOwedByB + taxAmountB + gratuityAmountB : 0;
                totalOwedByA += activityTotalOwedByA != 0 ? activityTotalOwedByA + taxAmountA + gratuityAmountA : 0;
            }

            return totalOwedByB - totalOwedByA;
        }



        /// <summary>
        /// Get a list of participant that are in activities
        /// </summary>
        /// <returns>List of participants ids</returns>
        public List<long> GetActiveParticipants()
        {
            return Activities
            .SelectMany(a => a.Items)
            .SelectMany(i => i.Payers)
            .Select(p => p.Payer.Id)
            .Distinct()
            .ToList();
        }
    }
}
