using ClassCreator.Data.Utility.DTO;

namespace ClassCreator.Data.Common
{
    /// <summary>
    /// Controller for handling <see cref="ObjectDataDto"/> and <see cref="Utility.Entity.ObjectData"/>
    /// </summary>
    public interface IObjectHandler
    {
        /// <summary>
        /// Verifies <see cref="ObjectDataDto"/> and if verification is success, adds new object.
        /// </summary>
        /// <param name="objectDataDto">The chosen dto to be added</param>
        /// <returns>True if addition has been successful; otherwise - false</returns>
        public bool Add(ObjectDataDto objectDataDto);
        /// <summary>
        /// Updates the chosen dto
        /// </summary>
        /// <param name="objectDataDto">Chosen dto to be updated</param>
        /// <returns>True if update has been successful; otherwise - false</returns>
        public bool Update(ObjectDataDto objectDataDto);
        /// <summary>
        /// Gets the <see cref="ObjectDataDto"/> by its name
        /// </summary>
        /// <param name="typeName">Name of <see cref="ObjectDataDto"/></param>
        /// <returns>An instance of <see cref="ObjectDataDto"/> if it's been found; otherwise - NULL</returns>
        public ObjectDataDto? Get(string typeName);
        /// <summary>
        /// Gets all contained <see cref="ObjectDataDto"/> instances
        /// </summary>
        /// <returns>An instance of <see cref="IEnumerable{T}"/> which has all contained <see cref="ObjectDataDto"/></returns>
        public IEnumerable<ObjectDataDto> GetAll();
        /// <summary>
        /// Removes <see cref="ObjectDataDto"/> by its name
        /// </summary>
        /// <param name="typeName">Name to find instance of <see cref="ObjectDataDto"/></param>
        /// <returns>True if <see cref="ObjectDataDto"/> has been removed successfully; otherwise - false</returns>
        public bool Remove(string typeName);
        /// <summary>
        /// Tries to get an instance as an <see cref="object"/> of type that is contained
        /// </summary>
        /// <param name="typeName">Name of type that is contained</param>
        /// <returns>An instance as an <see cref="object"/> of chosen type if it's contained; otherwise - NULL</returns>
        public object? TryGetInstance(string typeName);
    }
}
