//***********************************************************************************
//Program: Activity.cs
//Description: Activity model
//Date: Sep 25, 2025
//Author: John Nasitem
//***********************************************************************************



namespace takethetab_server.Domain.Entities
{
    public class Activity(long id, string name, bool isGratuityTypePercent, decimal gratuityAmount, bool addFivePercentTax, Event parentEvent, User payee, List<ActivityItem> items)
    {
        /// <summary>
        /// Id of the activity
        /// </summary>
        public long Id { get; set; } = id;



        /// <summary>
        /// Name of the activity
        /// </summary>
        public string Name { get; set; } = name;



        /// <summary>
        /// Is the gratuity type percent
        /// </summary>
        public bool IsGratuityTypePercent { get; set; } = isGratuityTypePercent;



        /// <summary>
        /// Gratuity amount
        /// </summary>
        public decimal GratuityAmount { get; set; } = gratuityAmount;



        /// <summary>
        /// Should 5% tax be added to the costs
        /// </summary>
        public bool AddFivePercentTax { get; set; } = addFivePercentTax;



        /// <summary>
        /// Parent event of the activity
        /// </summary>
        public Event ParentEvent { get; set; } = parentEvent;



        /// <summary>
        /// Person who paid for the event and needs to be paid back
        /// </summary>
        public User Payee { get; set; } = payee;



        /// <summary>
        /// Items inside of the event
        /// </summary>
        public List<ActivityItem> Items { get; set; } = items;



        /// <summary>
        /// How much the specified user is owed by the payers
        /// </summary>
        /// <param name="userId">Id of user</param>
        /// <returns>Amount the user is owed</returns>
        public decimal TotalAmountOwed(long userId)
        {
            if (Payee.Id != userId)
                return 0;

            decimal totalAmount = Items.Sum(i => i.TotalAmountOwed(userId));

            if (totalAmount == 0)
                return 0;

            int payerCount = Items.SelectMany(i => i.Payers)
                                  .Select(p => p.Payer.Id)
                                  .Distinct()
                                  .Count();

            decimal taxAmount = AddFivePercentTax ? totalAmount * 0.05m : 0;
            decimal gratuityAmount = IsGratuityTypePercent ? totalAmount * (GratuityAmount / 100m) : GratuityAmount / payerCount;

            return totalAmount + taxAmount + gratuityAmount;
        }



        /// <summary>
        /// How much the specified user owes to the activity payee
        /// </summary>
        /// <param name="userId">Id of user</param>
        /// <returns>Amount the user owes</returns>
        public decimal TotalAmountOwing(long userId)
        {
            if (Payee.Id == userId)
                return 0;

            decimal totalAmount = Items.Sum(item => item.TotalAmountOwing(userId));

            if (totalAmount == 0)
                return 0;

            int payerCount = Items.SelectMany(i => i.Payers)
                                  .Select(p => p.Payer.Id)
                                  .Distinct()
                                  .Count();

            decimal taxAmount = AddFivePercentTax ? totalAmount * 0.05m : 0;
            decimal gratuityAmount = IsGratuityTypePercent ? totalAmount * (GratuityAmount / 100m) : GratuityAmount / payerCount;

            return totalAmount + taxAmount + gratuityAmount;
        }



        /// <summary>
        /// Has the specified payer paid everything back to the activity payee
        /// </summary>
        /// <param name="payerId">Id of payer user</param>
        /// <returns>True if payer paid everything back, otherwise false</returns>
        public bool HasPayerFullyRepaid(long payerId)
        {
            return Items.SelectMany(i => i.Payers).All(i => i.Payer.Id == payerId && i.HasPaid);
        }



        /// <summary>
        /// Has all the payments been confirmed for the specified payer
        /// </summary>
        /// <param name="payerId">Id of payer user</param>
        /// <returns>True if all payments have been confirmed, otherwise false</returns>
        public bool HasPaymentsBeenConfirmed(long payerId)
        {
            return Items.SelectMany(i => i.Payers).All(i => i.Payer.Id == payerId && i.PaymentConfirmed);
        }



        /// <summary>
        /// Set the hasPaid flag for all activity items
        /// </summary>
        /// <param name="payerId">Id of payer</param>
        /// <param name="hasPaid">New flag value</param>
        public void SetHasPaid(long payerId, bool hasPaid)
        {
            foreach (ActivityItemPayer itemPayer in Items.SelectMany(i => i.Payers))
                if (itemPayer.Payer.Id == payerId)
                    itemPayer.HasPaid = hasPaid;
        }



        /// <summary>-
        /// Set the payment confirmed flag for all activity items
        /// </summary>
        /// <param name="payerId">Id of payer</param>
        /// <param name="paymentConfirmed">New flag value</param>
        public void SetPaymentConfirmed(long payerId, bool paymentConfirmed)
        {
            foreach (ActivityItemPayer itemPayer in Items.SelectMany(i => i.Payers))
                if (itemPayer.Payer.Id == payerId)
                    itemPayer.PaymentConfirmed = paymentConfirmed;
        }



        /// <summary>
        /// Get a list of payers and their summed owing amounts
        /// </summary>
        /// <returns>List of payers</returns>
        public List<ActivityItemPayer> GetPayers()
        {
            Dictionary<long, ActivityItemPayer> payersDict = [];

            foreach (ActivityItemPayer payer in Items.SelectMany(i => i.Payers))
            {
                if (!payersDict.ContainsKey(payer.Payer.Id))
                {
                    payersDict.Add(payer.Payer.Id, new ActivityItemPayer(payer));
                    continue;
                }

                payersDict[payer.Payer.Id].AmountOwing += payer.AmountOwing;
            }

            int payerCount = Items.SelectMany(i => i.Payers)
                                  .Select(p => p.Payer.Id)
                                  .Distinct()
                                  .Count();

            foreach (var payerKvp in payersDict)
            {
                decimal taxAmount = AddFivePercentTax ? payerKvp.Value.AmountOwing * 0.05m : 0;
                decimal gratuityAmount = IsGratuityTypePercent ? payerKvp.Value.AmountOwing * (GratuityAmount / 100m) : GratuityAmount / payerCount;
                payerKvp.Value.AmountOwing = payerKvp.Value.AmountOwing + taxAmount + gratuityAmount;
            }

            return payersDict.Select(kvp => kvp.Value).ToList();
        }
    }
}
