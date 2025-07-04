namespace LibraryRestModels
{
    public class BooksREST
    {
        public Guid Id { get; set; }
        public string Title { get; set; }

        public string Isbn { get; set; }

        public int PublishedYear { get; set; }

        public int Price { get; set; }

        public string BookAuthors { get; set; }

    }
}
