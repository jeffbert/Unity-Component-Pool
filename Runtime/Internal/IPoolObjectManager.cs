namespace Bert.Pool.Internal
{
    internal interface IPoolObjectManager
    {
        void Pool(PoolObject poolObject);
        void Destroy(PoolObject poolObject);
    }
}