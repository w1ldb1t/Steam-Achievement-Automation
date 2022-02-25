namespace Agent
{
    class Achievement
    {
        private string Name;
        private string Id;

        public Achievement(string name, string id)
        {
            this.Name = name;
            this.Id = id;
        }

        public string GetDisplayName()
        {
            return Name;
        }

        public string GetId()
        {
            return Id;
        }
    }
}
