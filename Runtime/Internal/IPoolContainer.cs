namespace Bert.Pool.Internal
{
    internal interface IPoolContainer
    {
        void Pool(PoolObject poolObject);
        void Destroy(PoolObject poolObject);
    }
}