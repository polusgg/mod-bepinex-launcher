namespace ClientLauncher.Models.Cosmetics
{
    public class GenericResponse<T>
    {
        public bool Ok { get; set; }
        public T Data { get; set; }
    }
}