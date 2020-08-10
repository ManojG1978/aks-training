namespace VotingApp.Models 
{
    public class VoteViewModel
    {
        public VoteViewModel(int vote1 = 0, int vote2 = 0)
        {
            Vote1 = vote1;
            Vote2 = vote2;
        }
        public int Vote1 { get; private set; }

        public int Vote2 { get; private set; }

    }
}