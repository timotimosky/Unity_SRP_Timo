namespace pool
{
    public interface IPoolable
    {
        void New();

        void Free();
    }
}