namespace Helper.Jobs
{
    public interface ICryptoEngine
    {
        byte[] Encrypt(byte[] data);

        byte[] Decrypt(byte[] data);
    }
}
