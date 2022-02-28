namespace Agent
{
    class Achievement
    {
        public string Id { get; private set; }
        public double Percent { get; private set; }
        public bool Unlocked { get; private set; }

        public Achievement(string id, double percent, bool unlocked)
        {
            this.Id = id;
            this.Percent = percent;
            this.Unlocked = unlocked;
        }
    }
}
