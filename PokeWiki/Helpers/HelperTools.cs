namespace MvcCoreCryptography.Helpers
{
    public class HelperTools
    {
        public static string GenerarSalt()
        {
            Random random = new Random();
            string salt = "";
            for (int i = 1; i <= 50; i++)
            {
                int num = random.Next(1, 255);
                char letra = Convert.ToChar(num);
                salt += letra;
            }
            return salt;
        }

        public static bool CompareArrays(byte[] a, byte[] b)
        {
            bool iguales = true;
            if(a.Length != b.Length)
            {
                iguales = false;
            } else
            {
                // Comparamos byte a byte
                for (int i = 0;  i < a.Length; i++)
                {
                    if (a[i].Equals(b[i]) == false)
                    {
                        iguales = false;
                        break;
                    }
                }
            }
            return iguales;
        }
    }
}
