namespace ClassCreator.Data.Common
{
    public interface IObjectContainer<T> where T : IObjectDataEntity
    {
        /// <summary>
        /// Saves (if instance is not contained) or updates (if current instance already contained) chosen <see cref="T"/>
        /// </summary>
        /// <param name="objectData">Chosen instance to be saved</param>
        /// <returns>True if saving or updating has been successful; otherwise - false</returns>
        public bool SaveOrUpdate(T objectData);
        /// <summary>
        /// Removes chosen instance of <see cref="T"/> from the collection
        /// </summary>
        /// <param name="objectData">Chosen instance to be removed</param>
        /// <param name="cancellationToken">Cancellation token to stop operation</param>
        /// <returns>True if deletion has been successful, otherwise - false</returns>
        public bool Remove(T objectData, CancellationToken cancellationToken = default);
        /// <summary>
        /// Determines if chosen instance of <see cref="T"/> is contained in the collection
        /// </summary>
        /// <param name="objectData">Instance to be determined</param>
        /// <returns>True if chosen instance is contained; otherwise - false</returns>
        public bool Contains(T objectData);
        /// <summary>
        /// Returns all contained instances of <see cref="T"/>
        /// </summary>
        /// <returns>A new instance of <see cref="IEnumerable{T}"/> with contained all instances of <see cref="T"/> inside</returns>
        public IEnumerable<T> GetAll();
        /// <summary>
        /// Returns an instance of <see cref="T"/> by its own name
        /// </summary>
        /// <param name="typeName">The name of <see cref="T"/></param>
        /// <returns>An instance of <see cref="T"/> if there exists one; otherwise - null</returns>
        public T? Get(string typeName);
    }
}
