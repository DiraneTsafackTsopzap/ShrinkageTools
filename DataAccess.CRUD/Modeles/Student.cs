namespace DataAccess.CRUD.Modeles
{
    public class Student
    {
        public Guid EtudiantId { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Nom { get; set; } = string.Empty;

        public string Prenom { get; set; } = string.Empty;

        public string Telephone { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
