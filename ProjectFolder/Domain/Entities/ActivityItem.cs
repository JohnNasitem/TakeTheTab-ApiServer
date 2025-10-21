//***********************************************************************************
//Program: ActivityItem.cs
//Description: Activity item model
//Date: Oct 10, 2025
//Author: John Nasitem
//***********************************************************************************



namespace takethetab_server.Domain.Entities
{
    public class ActivityItem(long itemId, Activity parentActivity, string name, decimal cost, bool isSplitTypeEvenly, List<ActivityItemPayer> payers)
    {
        /// <summary>
        /// Id of activity item
        /// </summary>
        public long Id { get; set; } = itemId;



        /// <summary>
        /// Activity the item is under
        /// </summary>
        public Activity ParentActivity { get; set; } = parentActivity;



        /// <summary>
        /// Name of activity
        /// </summary>
        public string Name { get; set; } = name;



        /// <summary>
        /// Cost of activity
        /// </summary>
        public decimal Cost { get; set; } = cost;



        /// <summary>
        /// Is the cost of the item split evenly between the payers
        /// </summary>
        public bool IsSplitTypeEvenly { get; set; } = isSplitTypeEvenly;



        /// <summary>
        /// Payers for the item
        /// </summary>
        public List<ActivityItemPayer> Payers { get; set; } = payers;




        /// <summary>
        /// How much the specified user is owed by the payers
        /// </summary>
        /// <param name="userId">Id of user</param>
        /// <returns>Amount the user is owed</returns>
        public decimal TotalAmountOwed(long userId)
        {
            if (ParentActivity.Payee.Id == userId)
                return Payers.Where(p => p.Payer.Id != userId).Sum(p => p.AmountOwing);

            return 0;
        }



        /// <summary>
        /// How much the specified user owes to the activity payee
        /// </summary>
        /// <param name="userId">Id of user</param>
        /// <returns>Amount the user owes</returns>
        public decimal TotalAmountOwing(long userId)
        {
            if (ParentActivity.Payee.Id == userId)
                return 0;

            ActivityItemPayer? payer = Payers.Find(p => p.Payer.Id == userId);

            if (payer != null)
                return payer.AmountOwing;

            return 0;
        }



        public override bool Equals(object? obj)
        {
            if (obj is not ActivityItem other)
                return false;

            return
                other.Id == Id &&
                other.ParentActivity.Id == ParentActivity.Id &&
                other.Name == Name &&
                other.Cost == Cost &&
                other.IsSplitTypeEvenly == IsSplitTypeEvenly &&
                other.Payers.Count == Payers.Count &&
                other.Payers.Except(Payers).Any();
        }



        public override int GetHashCode() => 1;
    }



    public class ActivityItemPayer
    {
        /// <summary>
        /// User who needs to pay for the item
        /// </summary>
        public User Payer { get; set; } = null!;



        /// <summary>
        /// Amount the payer owes
        /// </summary>
        public decimal AmountOwing { get; set; }



        /// <summary>
        /// Has the payer made payment
        /// </summary>
        public bool HasPaid { get; set; }



        /// <summary>
        /// Has payee confirmed the payment
        /// </summary>
        public bool PaymentConfirmed { get; set; }



        public ActivityItemPayer() { }



        /// <summary>
        /// Create a hard copy of the specified instance
        /// </summary>
        /// <param name="other">Instance to hard copy</param>
        public ActivityItemPayer(ActivityItemPayer other)
        {
            Payer = other.Payer;
            AmountOwing = other.AmountOwing;
            HasPaid = other.HasPaid;
            PaymentConfirmed = other.PaymentConfirmed;
        }



        public override bool Equals(object? obj)
        {
            if (obj is not ActivityItemPayer other)
                return false;

            return
                other.Payer.Id == Payer.Id &&
                other.AmountOwing == AmountOwing &&
                other.HasPaid == HasPaid &&
                other.PaymentConfirmed == PaymentConfirmed;
        }



        public override int GetHashCode() => 1;
    }
}
