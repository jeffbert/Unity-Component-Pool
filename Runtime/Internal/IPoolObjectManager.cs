namespace Bert.Pool.Internal
{
    internal interface IPoolObjectManager
    {
        /// <summary>
        /// Remove the <see cref="PoolObject"/> instance from the pool.
        /// </summary>
        /// <param name="poolObject">Instance to remove from the pool.</param>
        void UnPool(PoolObject poolObject);
        
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