namespace ClassCreator.Data.Common
{
    /// <summary>
    /// Controller for handling <see cref="T"/> and <see cref="IObjectDataEntity"/>
    /// </summary>
    public interface IObjectHandler<T> where T : IObjectDataDto
    {
        /// <summary>
        /// Verifies <see cref="T"/> and if verification is success, adds new object.
        /// </summary>
        /// <param name="objectDataDto">The chosen dto to be added</param>
        /// <returns>True if addition has been successful; otherwise - false</returns>
        public bool Add(T objectDataDto);
        /// <summary>
        /// Updates the chosen dto
        /// </summary>
        /// <param name="objectDataDto">Chosen dto to be updated</param>
        /// <returns>True if update has been successful; otherwise - false</returns>
        public bool Update(T objectDataDto);
        /// <summary>
        /// Gets the <see cref="T"/> by its name
        /// </summary>
        /// <param name="typeName">Name of <see cref="T"/></param>
        /// <returns>An instance of <see cref="T"/> if it's been found; otherwise - NULL</returns>
        public T? Get(string typeName);
        /// <summary>
        /// Gets all contained <see cref="T"/> instances
        /// </summary>
        /// <returns>An instance of <see cref="IEnumerable{T}"/> which has all contained <see cref="T"/></returns>
        public IEnumerable<T> GetAll();
        /// <summary>
        /// Removes <see cref="T"/> by its name
        /// </summary>
        /// <param name="typeName">Name to find instance of <see cref="T"/></param>
        /// <returns>True if <see cref="T"/> has been removed successfully; otherwise - false</returns>
        public bool Remove(string typeName);
    }
}
