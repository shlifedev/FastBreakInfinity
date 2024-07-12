namespace LD.Numeric
{
    public class AlphabetUtility
    {
        public static bool IsAlphabet(char input)
        {
            if (input >= 'A' && input <= 'Z')
                return true;
            return false;
        }
    }
}