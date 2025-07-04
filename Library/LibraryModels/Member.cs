namespace LibraryModels
{
    public class Member
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime MembershipDate { get; set; }

        public int BirthYear { get; set; }

        public ICollection<Loan>? Loans { get; set; }
    }
}
