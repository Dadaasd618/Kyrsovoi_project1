namespace PersonnelManagement.Models
{
    public class LookupItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public override string ToString() => Name;
    }
}