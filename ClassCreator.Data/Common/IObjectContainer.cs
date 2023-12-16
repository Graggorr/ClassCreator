using ClassCreator.Data.Utility.Entity;

namespace ClassCreator.Data.Common
{
    internal interface IObjectContainer
    {
        /// <summary>
        /// Saves (if instance is not contained) or updates (if current instance already contained) chosen <see cref="ObjectData"/>
        /// </summary>
        /// <param name="objectData">Chosen instance to be saved</param>
        /// <returns>True if saving or updating has been successful; otherwise - false</returns>
        public bool SaveOrUpdate(ObjectData objectData);
        /// <summary>
        /// Removes chosen instance of <see cref="ObjectData"/> from the collection
        /// </summary>
        /// <param name="objectData">Chosen instance to be removed</param>
        /// <param name="cancellationToken">Cancellation token to stop operation</param>
        /// <returns>True if deletion has been successful, otherwise - false</returns>
        public bool Remove(ObjectData objectData, CancellationToken cancellationToken = default);
        /// <summary>
        /// Determines if chosen instance of <see cref="ObjectData"/> is contained in the collection
        /// </summary>
        /// <param name="objectData">Instance to be determined</param>
        /// <returns>True if chosen instance is contained; otherwise - false</returns>
        public bool Contains(ObjectData objectData);
        /// <summary>
        /// Returns all contained instances of <see cref="ObjectData"/>
        /// </summary>
        /// <returns>A new instance of <see cref="IEnumerable{T}"/> with contained all instances of <see cref="ObjectData"/> inside</returns>
        public IEnumerable<ObjectData> GetAll();
        /// <summary>
        /// Returns an instance of <see cref="ObjectData"/> by its own name
        /// </summary>
        /// <param name="typeName">The name of <see cref="ObjectData"/></param>
        /// <returns>An instance of <see cref="ObjectData"/> if there exists one; otherwise - null</returns>
        public ObjectData? Get(string typeName);
    }
}
