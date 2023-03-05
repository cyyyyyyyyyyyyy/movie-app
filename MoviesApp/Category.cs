namespace MoviesApp
{
    public class Category
    {
        public int categoryId { get; set; }
        public Movie movie { get; set; }
        public Person person { get; set; }
        public string category { get; set; }

        public Category() { categoryId = 0; movie = new(); person = new(); category = ""; }

        public override bool Equals(object? obj)
        {
            if (obj is Category)
            {
                return person.Equals(obj as Person);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return person.GetHashCode();
        }
    }
}