namespace ClientLauncher.Models.Cosmetics
{
    public class CosmeticsGenericResponse<T>
    {
        public bool Ok { get; set; }
        public T Data { get; set; }
    }
}