//***********************************************************************************
//Program: CreateActivityItemComparer.cs
//Description: Comparer for CreateActivityItem instances
//Date: Oct 17, 2025
//Author: John Nasitem
//***********************************************************************************



using takethetab_server.Application.Dtos.Activities.CreateActivity;

namespace takethetab_server.Domain.Comparers
{
    // Class created with the help of chat gpt
    public class CreateActivityItemComparer : IEqualityComparer<CreateActivityItem>
    {
        public bool Equals(CreateActivityItem? x, CreateActivityItem? y)
        {
            if (x == null || y == null) return false;

            if (x.ItemName != y.ItemName ||
                x.ItemCost != y.ItemCost ||
                x.IsSplitTypeEvenly != y.IsSplitTypeEvenly ||
                x.Payers.Count != y.Payers.Count)
                return false;

            foreach (var kv in x.Payers)
            {
                if (!y.Payers.TryGetValue(kv.Key, out var amount) || amount != kv.Value)
                    return false;
            }

            return true;
        }



        public int GetHashCode(CreateActivityItem obj)
        {
            // Combine item fields + payer info into a stable hash
            int hash = HashCode.Combine(obj.ItemName, obj.ItemCost, obj.IsSplitTypeEvenly);

            foreach (var kv in obj.Payers.OrderBy(p => p.Key)) // order ensures consistent hash
                hash = HashCode.Combine(hash, kv.Key, kv.Value);

            return hash;
        }
    }
}
