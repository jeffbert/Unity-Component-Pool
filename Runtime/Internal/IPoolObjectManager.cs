namespace Bert.Pool.Internal
{
    internal interface IPoolObjectManager
    {
        /// <summary>
        /// Add <see cref="PoolObject"/> instance back to the pool.
        /// </summary>
        /// <param name="poolObject">Instance to add to the pool.</param>
        void Pool(PoolObject poolObject);
        
        /// <summary>
        /// Destroy a <see cref="PoolObject"/> instance.
        /// </summary>
        /// <param name="poolObject">Instance to destroy.</param>
        void Destroy(PoolObject poolObject);
    }
}