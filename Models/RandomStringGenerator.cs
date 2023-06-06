namespace Library.Models;

public class RandomStringGenerator
{
    public static string GeneratePassword(int length = 9)
    {
        const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*?_-";
        var random = new Random();

            var chars = new char[length+2];
            for (int i = 0; i < length; i++)
            {
                chars[i] = validChars[random.Next(validChars.Length)];
            }
            chars[length-2] = 'b';
            chars[length-1] = 'A';
            chars[length] = '0';
            chars[length+1] = '!';
            return new string(chars);
    }
}
